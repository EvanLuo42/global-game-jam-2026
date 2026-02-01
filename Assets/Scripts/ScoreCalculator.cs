using DefaultNamespace;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreCalculator : MonoBehaviour
{
    [Header("Input")]
    public RenderTexture playerMask;

    [Header("Params")]
    [Range(0f, 1f)]
    public float attentionThreshold = 0.05f;

    [Header("Weights")]
    public float invertWeight = 2.0f;

    public void CalculateScore()
    {
        var playerTex = Readback(playerMask);

        var width = playerTex.width;
        var height = playerTex.height;

        var attentionCount = 0;
        var attentionHit = 0;

        var justiceSum = 0f;
        var justiceWeight = 0f;

        var injusticeSum = 0f;
        var injusticeWeight = 0f;

        // for (var y = 0; y < height; y++)
        // {
        //     for (var x = 0; x < width; x++)
        //     {
        //         var p = playerTex.GetPixel(x, y);
        //
        //         var ij = GlobalState.CurrentLevel.injusticeMask.GetPixel(x, y).r;
        //
        //         var region = Mathf.Max(j, ij);
        //         if (region < 0.5f)
        //             continue;
        //
        //         attentionCount++;
        //         var totalEdit = p.r + p.g + p.b + p.a;
        //         if (totalEdit > attentionThreshold)
        //             attentionHit++;
        //         
        //         var justiceForce = p.g; // Lighten
        //         var injusticeForce = p.r + p.b + p.a * invertWeight;
        //
        //         var stance = Mathf.Clamp(justiceForce - injusticeForce, -1f, 1f);
        //
        //         if (j > 0.5f)
        //         {
        //             justiceSum += stance * j;
        //             justiceWeight += j;
        //         }
        //
        //         if (!(ij > 0.5f)) continue;
        //         injusticeSum += -stance * ij;
        //         injusticeWeight += ij;
        //     }
        // }

        GlobalState.ScoreResult = new ScoreResult
        {
            attention = attentionCount > 0 ? (float)attentionHit / attentionCount : 0f,
            justice = justiceWeight > 0 ? justiceSum / justiceWeight : 0f,
            injustice = injusticeWeight > 0 ? injusticeSum / injusticeWeight : 0f
        };

        SceneManager.LoadScene("ScoreScene");
    }

    private static Texture2D Readback(RenderTexture rt)
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
