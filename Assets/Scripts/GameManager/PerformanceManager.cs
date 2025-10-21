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

    public void RegisterWordResult(bool success, float timeTaken, int charactersTyped)
    {
        totalWords++;

        if (success)
        {
            successfulWords++;
            currentStreak++;
            maxStreak = Mathf.Max(maxStreak, currentStreak);
            //Debug.Log($"successfulWords={successfulWords}, timeTaken={timeTaken:F3}, chars={charactersTyped}");
            UpdateAverageSpeed(timeTaken, charactersTyped);
            Debug.Log($"averageSpeed={averageSpeed:F3}");
            //Debug.Log($"timeTaken={timeTaken:F2}s, chars={charactersTyped}, CPS={(charactersTyped / timeTaken):F2}");
        }
        else
        {
            currentStreak = 0;
        }

        CalculatePerformanceScore();
    }

    private int totalCharactersTyped = 0;
    private void UpdateAverageSpeed(float timeTaken, int charactersTyped)
    {
        totalCharactersTyped += charactersTyped;
        float cps = (float)charactersTyped / timeTaken;
        Debug.Log($"chars={charactersTyped}, timeTaken={timeTaken:F3}, cps={cps:F3}");

        averageSpeed = ((averageSpeed * (successfulWords - 1)) + cps) / successfulWords;
        //Debug.Log($"[Performance] timeTaken={timeTaken:F2}s, chars={charactersTyped}, CPS={cps:F2}, AveSpeed = {averageSpeed:F2} ");
    }

    private void CalculatePerformanceScore()
    {
        float targetCPS = 4f;
        float speedScore = Mathf.Clamp01(averageSpeed / targetCPS);
        float streakScore = Mathf.Clamp01((float)maxStreak / 20f);//(float)maxStreak / 20f;//maxStreak * 0.05f; 
        float accuracy = (totalWords > 0) ? (float)successfulWords / totalWords : 0f;
        float accuracyScore = Mathf.Clamp01(accuracy);

        performanceScore = (speedScore * 0.4f) + (streakScore * 0.3f) + (accuracyScore * 0.3f);
        performanceScore = Mathf.Clamp01(performanceScore);
        Debug.Log($"[Performance] Speed={speedScore:F2}, Streak={streakScore:F2}, Accuracy={accuracyScore:F2}, Final={performanceScore:F2}");
    }

    public string GetPerformanceTier()
    {
        var profile = difficultyProfile.GetProfile(performanceScore);
        return profile != null ? profile.profileName : "Unknown";
    }
}
