using UnityEngine;

public class PerformanceManager : MonoBehaviour
{
    [Header("Difficulty Settings")]
    public DifficultyProfile difficultyProfile;

    [Header("Performance Data")]
    public float averageSpeed;
    public int currentStreak;
    public int maxStreak;
    public int totalWords;
    public int successfulWords;
    public float performanceScore;

    public void RegisterWordResult(bool success, float timeTaken)
    {
        totalWords++;

        if (success)
        {
            successfulWords++;
            currentStreak++;
            maxStreak = Mathf.Max(maxStreak, currentStreak);
            UpdateAverageSpeed(timeTaken);
        }
        else
        {
            currentStreak = 0;
        }

        CalculatePerformanceScore();
    }

    private void UpdateAverageSpeed(float timeTaken)
    {
        averageSpeed = ((averageSpeed * (successfulWords - 1)) + timeTaken) / successfulWords;
    }

    private void CalculatePerformanceScore()
    {
        float speedScore = Mathf.Clamp01(1f - (averageSpeed / 4f));
        float streakScore = Mathf.Clamp01((float)maxStreak / 10f);
        float accuracy = (float)successfulWords / totalWords;
        float accuracyScore = Mathf.Clamp01(accuracy);

        performanceScore = (speedScore * 0.4f) + (streakScore * 0.3f) + (accuracyScore * 0.3f);
        Debug.Log("Performance Score" + performanceScore);
    }

    public string GetPerformanceTier()
    {
        var profile = difficultyProfile.GetProfile(performanceScore);
        return profile != null ? profile.profileName : "Unknown";
    }
}
