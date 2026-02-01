using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level Score Calculator
/// 
/// Player Mask Channels:
/// - R = Darken  (Injustice when matching required edit)
/// - G = Lighten (Justice - always counts as rebellion)
/// - B = Blur    (Injustice when matching required edit)
/// - A = Invert  (Injustice when matching required edit)
/// 
/// InjusticeMask (how to create):
/// - RED opaque area (R high, B low, A=1) = Should be edited with Darken
/// - BLUE opaque area (B high, R low, A=1) = Should be edited with Blur  
/// - TRANSPARENT area (A < 0.5) = Should be edited with Invert
/// - BLACK opaque area (R=0, B=0, A=1) = No edit required, skip
/// 
/// Scoring Rules:
/// 1. Sales Rate = Valid Edited Area / Total Editable Area
///    (Only CORRECT edits or Lighten count - wrong tool = no sales)
/// 2. Boss Satisfaction = Injustice Area / Total Editable Area
///    (Player used the CORRECT edit type as required by injusticeMask)
/// 3. Justice Value = Justice Area / Total Editable Area
///    (Player used Lighten instead of following orders)
/// 
/// IMPORTANT: Using wrong tool (e.g., Darken in Blur area) = NO score at all!
/// </summary>
public class ScoreCalculator : MonoBehaviour
{
    [Header("Input (Optional - will auto-find if not set)")]
    [Tooltip("Player's mask RenderTexture (MaskRT0)")]
    public RenderTexture playerMaskRT0;

    [Header("Params")]
    [Tooltip("Threshold for determining if a pixel has been edited")]
    [Range(0f, 1f)]
    public float editThreshold = 0.1f;

    [Header("Debug")]
    public bool debugMode = true;

    /// <summary>
    /// Calculate current level score and transition to score scene
    /// </summary>
    public void CalculateScore()
    {
        var level = GlobalState.CurrentLevel;
        if (level == null)
        {
            Debug.LogError("ScoreCalculator: CurrentLevel is null!");
            GlobalState.ScoreResult = new ScoreResult();
            GlobalState.RecordCurrentLevelResult();
            SceneManager.LoadScene("ScoreScene");
            return;
        }

        // Auto-find RenderTexture if not manually set
        if (playerMaskRT0 == null)
        {
            var maskPainter = FindFirstObjectByType<MaskPainter>();
            if (maskPainter != null)
            {
                playerMaskRT0 = maskPainter.maskRT0;
                if (debugMode)
                    Debug.Log("ScoreCalculator: Auto-found maskRT0 from MaskPainter");
            }
        }

        if (playerMaskRT0 == null)
        {
            Debug.LogError("ScoreCalculator: Cannot find playerMaskRT0!");
            GlobalState.ScoreResult = CreateDefaultScore();
            GlobalState.RecordCurrentLevelResult();
            SceneManager.LoadScene("ScoreScene");
            return;
        }

        // Read player mask
        var playerMask = ReadbackRT(playerMaskRT0);
        
        // Get injustice mask (defines required edit types per region)
        var injusticeMask = level.injusticeMask;
        
        if (injusticeMask == null)
        {
            Debug.LogWarning("ScoreCalculator: injusticeMask is null, using simple calculation");
            var result = CalculateSimple(playerMask);
            GlobalState.ScoreResult = result;
            GlobalState.RecordCurrentLevelResult();
            Destroy(playerMask);
            SceneManager.LoadScene("ScoreScene");
            return;
        }

        // Check if texture is readable
        if (!injusticeMask.isReadable)
        {
            Debug.LogError($"ScoreCalculator: {injusticeMask.name} is not readable! Enable Read/Write in import settings.");
            var result = CalculateSimple(playerMask);
            GlobalState.ScoreResult = result;
            GlobalState.RecordCurrentLevelResult();
            Destroy(playerMask);
            SceneManager.LoadScene("ScoreScene");
            return;
        }

        // Calculate score with channel matching
        var scoreResult = CalculateWithChannelMatching(playerMask, injusticeMask);
        
        GlobalState.ScoreResult = scoreResult;
        GlobalState.RecordCurrentLevelResult();

        if (debugMode)
        {
            Debug.Log($"=== Level Score ===");
            Debug.Log($"Total Editable Area: {scoreResult.totalEditableArea}");
            Debug.Log($"Total Edited Area: {scoreResult.totalEditedArea}");
            Debug.Log($"Injustice Area (correct edit): {scoreResult.injusticeArea}");
            Debug.Log($"Justice Area (used Lighten): {scoreResult.justiceArea}");
            Debug.Log($"Sales Rate: {scoreResult.salesRate:P1}");
            Debug.Log($"Boss Satisfaction: {scoreResult.bossSatisfaction:P1}");
            Debug.Log($"Justice Value: {scoreResult.justiceValue:P1}");
        }

        Destroy(playerMask);
        SceneManager.LoadScene("ScoreScene");
        TransitionController.Instance.TransitionToScene("ScoreScene");
    }

    private ScoreResult CreateDefaultScore()
    {
        return new ScoreResult
        {
            totalEditableArea = 1,
            totalEditedArea = 0,
            injusticeArea = 0,
            justiceArea = 0,
            salesRate = 0f,
            bossSatisfaction = 0f,
            justiceValue = 0f
        };
    }

    /// <summary>
    /// Simple calculation without channel matching
    /// </summary>
    private ScoreResult CalculateSimple(Texture2D playerMask)
    {
        var result = new ScoreResult();
        
        var width = playerMask.width;
        var height = playerMask.height;
        result.totalEditableArea = width * height;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var p = playerMask.GetPixel(x, y);
                
                var darken = p.r;
                var lighten = p.g;
                var blur = p.b;
                var invert = p.a;

                var injusticeStrength = Mathf.Max(darken, blur, invert);
                var justiceStrength = lighten;

                if (injusticeStrength <= editThreshold && justiceStrength <= editThreshold)
                    continue;

                result.totalEditedArea++;

                if (justiceStrength > injusticeStrength)
                    result.justiceArea++;
                else
                    result.injusticeArea++;
            }
        }

        result.CalculateRates();
        return result;
    }

    /// <summary>
    /// Calculate score with proper channel matching
    /// 
    /// InjusticeMask defines what edit is REQUIRED:
    /// - RED opaque area = Should use Darken
    /// - BLUE opaque area = Should use Blur
    /// - TRANSPARENT area = Should use Invert
    /// - BLACK opaque area = Not editable (skipped)
    /// 
    /// Scoring logic:
    /// - CORRECT edit type = +Sales, +Injustice (obeying orders)
    /// - Lighten = +Sales, +Justice (rebellion)
    /// - WRONG edit type = NO score (not counted at all)
    /// </summary>
    private ScoreResult CalculateWithChannelMatching(Texture2D playerMask, Texture2D injusticeMask)
    {
        var result = new ScoreResult();

        var width = playerMask.width;
        var height = playerMask.height;

        // Scale factors if textures have different sizes
        var scaleX = (float)injusticeMask.width / width;
        var scaleY = (float)injusticeMask.height / height;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                // Sample injustice mask to find required edit type
                var maskX = Mathf.Clamp(Mathf.FloorToInt(x * scaleX), 0, injusticeMask.width - 1);
                var maskY = Mathf.Clamp(Mathf.FloorToInt(y * scaleY), 0, injusticeMask.height - 1);
                var maskPixel = injusticeMask.GetPixel(maskX, maskY);

                // Check which edit type is required for this pixel
                // PRIORITY: Transparency (Invert) takes precedence over RGB channels
                // This handles PNG files where transparent areas may have RGB values
                var isTransparent = maskPixel.a < 0.5f;
                
                bool requiresDarken, requiresBlur, requiresInvert;
                
                if (isTransparent)
                {
                    // Transparent area = only Invert is required
                    requiresDarken = false;
                    requiresBlur = false;
                    requiresInvert = true;
                }
                else
                {
                    // Opaque area = check RGB channels
                    // Red = Darken, Blue = Blur
                    // Only count as Darken if it's predominantly red (R high, B low)
                    // Only count as Blur if it's predominantly blue (B high, R low)
                    requiresDarken = maskPixel.r > 0.5f && maskPixel.b < 0.5f;
                    requiresBlur = maskPixel.b > 0.5f && maskPixel.r < 0.5f;
                    requiresInvert = false;
                }

                // If no edit is required for this pixel, skip it
                // Black opaque areas (R=0, B=0, A=1) will be skipped
                var isEditableArea = requiresDarken || requiresBlur || requiresInvert;
                if (!isEditableArea)
                    continue;

                result.totalEditableArea++;

                // Sample player's edit
                var playerPixel = playerMask.GetPixel(x, y);
                var playerDarken = playerPixel.r;
                var playerLighten = playerPixel.g;
                var playerBlur = playerPixel.b;
                var playerInvert = playerPixel.a;

                // Check if player made any edit above threshold
                var hasPlayerDarken = playerDarken > editThreshold;
                var hasPlayerLighten = playerLighten > editThreshold;
                var hasPlayerBlur = playerBlur > editThreshold;
                var hasPlayerInvert = playerInvert > editThreshold;

                // Check if player used the CORRECT edit type for this area
                var usedCorrectEdit = false;
                if (requiresDarken && hasPlayerDarken)
                    usedCorrectEdit = true;
                else if (requiresBlur && hasPlayerBlur)
                    usedCorrectEdit = true;
                else if (requiresInvert && hasPlayerInvert)
                    usedCorrectEdit = true;

                // NEW LOGIC:
                // Sales (totalEditedArea) only increases if:
                // 1. Player used the CORRECT tool (Injustice) OR
                // 2. Player used Lighten (Justice)
                // Using WRONG tool (e.g., Darken in Invert area) = NO score at all

                if (hasPlayerLighten)
                {
                    // Player used Lighten = Justice (rebellion)
                    // This counts towards sales AND justice
                    result.totalEditedArea++;
                    result.justiceArea++;
                }
                else if (usedCorrectEdit)
                {
                    // Player used correct tool = Injustice (obeying orders)
                    // This counts towards sales AND boss satisfaction
                    result.totalEditedArea++;
                    result.injusticeArea++;
                }
                // If player used WRONG tool (not Lighten, not correct tool):
                // NO score - doesn't count towards sales, justice, or injustice
            }
        }

        result.CalculateRates();
        return result;
    }

    private static Texture2D ReadbackRT(RenderTexture rt)
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBAFloat, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        return tex;
    }
}
