using System;
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
    [SerializeField] float falseMultiplier;

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
            if (falseMultiplier < Math.Abs(0.3f))
            { 
                falseMultiplier += 0.1f;
                if (falseMultiplier > 0.3f) falseMultiplier = 0.3f; 
            }
        }

        CalculatePerformanceScore(success);
    }

    public void OnSessionSuccess()
    {
        if (falseMultiplier > 0) 
        { 
            falseMultiplier -= 0.15f;
            if (falseMultiplier < 0f) falseMultiplier = 0f; 
        }
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

    private void CalculatePerformanceScore(bool success)
    {
        float targetCPS = 4f;
        float speedScore = Mathf.Clamp01(averageSpeed / targetCPS);
        float streakScore = Mathf.Clamp01(currentStreak / 20f);//(float)maxStreak / 20f;//maxStreak * 0.05f; 
        float accuracy = (totalWords > 0) ? (float)successfulWords / totalWords : 0f;
        float accuracyScore = Mathf.Clamp01(MathF.Pow(accuracy, 2f));
        if (!success)
        {
            performanceScore -= ( 1 * falseMultiplier);
        }
        else
        {
            float hackyFalseMultiplier = falseMultiplier - 0.05f;
            performanceScore = (speedScore * 0.4f) + (streakScore * 0.3f) + (accuracyScore * 0.3f - hackyFalseMultiplier);
        }

        //Debug.LogError(1*falseMultiplier*accuracyScore);
        performanceScore = Mathf.Clamp01(performanceScore);
        Debug.LogError($"[Performance] Speed={speedScore:F2}, Streak={streakScore:F2}, Accuracy={accuracyScore:F2}, Final={performanceScore:F2}");
    }

    public string GetPerformanceTier()
    {
        var profile = difficultyProfile.GetProfile(performanceScore);
        return profile != null ? profile.profileName : "Unknown";
    }
}
