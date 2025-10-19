using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

[System.Serializable]
public class WordEntry
{
    public string word;
    public Difficulty difficulty;
    [Range(0f, 100f)] public float spawnChance = 100f; //chance in %
}

[CreateAssetMenu(fileName = "WordGlossary", menuName = "Scriptable Objects/WordGlossary")]
public class WordGlossary : ScriptableObject
{
    [SerializeField] List<WordEntry> words = new List<WordEntry>();

    public string GetRandomWordByDifficulty(Difficulty difficulty)
    {
        // Get list of words in the chosen difficulty
        var pool = words.FindAll(w => w.difficulty == difficulty);
        if (pool.Count == 0) return string.Empty;

        //Weighted random selection
        float totalChance = 0f;
        foreach (var entry in pool)
            totalChance += entry.spawnChance;
        
        float roll = Random.Range(0f, totalChance);
        foreach (var entry in pool)
        {
            if (roll < entry.spawnChance)
                return entry.word;
            roll -= entry.spawnChance;
        }

        return pool[pool.Count - 1].word;
    }

    public List<string> GetRandomWords(Difficulty difficulty, int count)
    {
        List<string> selected = new List<string>();
        for (int i = 0; i < count; i++)
        {
            string word = GetRandomWordByDifficulty(difficulty);
            if (!string.IsNullOrEmpty(word)) selected.Add(word);
            
        }
        return selected;
    }
}
