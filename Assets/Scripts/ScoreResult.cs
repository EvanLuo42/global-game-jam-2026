using UnityEngine;

namespace DefaultNamespace
{
    [System.Serializable]
    public struct ScoreResult
    {
        [Range(0f, 1f)]
        public float attention;

        [Range(-1f, 1f)]
        public float justice;

        [Range(-1f, 1f)]
        public float injustice;
    }
}