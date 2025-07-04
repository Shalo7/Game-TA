using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class TypingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject typingUI;
    [SerializeField] TMP_Text wordText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Slider timerSlider;

    [Header("Glossary")]
    [SerializeField] WordGlossary glossary;

    [Header("Settings")]
    [SerializeField] float timeLimit = 5f;

    private List<string> selectedWords = new();
    private int currentWordIndex = 0;
    private int correctWords = 0;
    private float timeRemaining;
    private bool isTyping = false;

    public delegate void OnTypingComplete(int correctCount);
    private OnTypingComplete onCompleteCallback;

    public void StartTyping(OnTypingComplete callback)
    {
        onCompleteCallback = callback;
        selectedWords = GetRandomWords(5);
        currentWordIndex = 0;
        correctWords = 0;
        timeRemaining = timeLimit;

        typingUI.SetActive(true);
        typingUI.GetComponent<CanvasGroup>().alpha = 1.0f;

        inputField.text = "";
        inputField.ActivateInputField();

        isTyping = true;

        ShowNextWord();
        StartCoroutine(UpdateTimer());
    }

    private List<string> GetRandomWords(int count)
    {
        List<string> copy = new(glossary.words);
        List<string> result = new();
        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int randIndex = Random.Range(0, copy.Count);
            result.Add(copy[randIndex]);
            copy.RemoveAt(randIndex);
        }
        return result;
    }

    private void ShowNextWord()
    {
        if (currentWordIndex >= selectedWords.Count)
        {
            FinishTyping();
            return;
        }

        wordText.text = selectedWords[currentWordIndex];
        inputField.text = "";
        inputField.ActivateInputField();
    }

    private IEnumerator UpdateTimer()
    {
        while (timeRemaining > 0f && isTyping)
        {
            timeRemaining -= Time.deltaTime;
            timerSlider.value = timeRemaining / timeLimit;

            //Check Input
            string expected = selectedWords[currentWordIndex];
            string typed = inputField.text;

            if (typed == expected)
            {
                Debug.Log("Correct!");
                correctWords++;
                currentWordIndex++;
                ShowNextWord();
            }
            else if (typed.Length >= expected.Length && typed != expected)
            {
                //Typo - skip word
                Debug.Log("Incorrect!");
                currentWordIndex++;
                ShowNextWord();
            }

            yield return null;
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        isTyping = false;
        typingUI.SetActive(false);
        onCompleteCallback?.Invoke(correctWords);
    }
}