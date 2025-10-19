using UnityEngine;

[System.Serializable]
public class DifficultyRatio
{
    public string profileName;
    public float threshold;
    public int easyCount;
    public int mediumCount;
    public int hardCount;
}

[CreateAssetMenu(fileName = "DifficultyProfile", menuName = "Scriptable Objects/DifficultyProfile")]
public class DifficultyProfile : ScriptableObject
{
    public DifficultyRatio[] difficultyProfiles;

    public DifficultyRatio GetProfile(float performanceScore)
    {
        DifficultyRatio bestMatch = null;

        foreach (var profile in difficultyProfiles)
        {
            if (performanceScore >= profile.threshold)
            {
                if (bestMatch == null || profile.threshold > bestMatch.threshold)
                {
                    bestMatch = profile;
                }
            }
        }
        return bestMatch;
    }
}
