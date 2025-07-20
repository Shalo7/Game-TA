using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using ParticleData.SpawnData;

public class TypewritingManager : MonoBehaviour
{
    [Header("Word Source")]
    public WordGlossary glossary;
    public List<string> wordList;

    [Header("UI References")]
    public TMP_Text shadowText_Player;
    public TMP_Text typedText_Player;
    public TMP_Text shadowText_Enemy;
    public TMP_Text typedText_Enemy;

    [Header("Font Settings")]
    public float baseFontSize = 36f;
    public float sizeIncreasePerLetter = 6f;

    [Header("Effects")]
    public GameObject wordCompleteParticle;
    public float shakeStrength = 10f;
    public float shakeDuration = 0.3f;
    public float bounceScale = 1.3f;
    public float bounceDuration = 0.4f;
    public float fadeOutDuration = 0.3f;

    [Header("Colors")]
    public bool useGradientForTypedText = false;
    public VertexGradient typedTextGradient;
    public Color shadowTextColor = new Color(1f, 1f, 1f, 22f / 255f);

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    public AudioClip wrongTypeSound;

    [Header("Early Fail Settings")]
    public float earlyFailDelay = 2f;

    [Header("Timer Integration")]
    public TypingWordTimer typingTimerUI;

    private int wordIndex = 0;
    private string currentWord;
    private string inputBuffer = "";
    private bool isTypingActive = false;
    private bool hasFailedEarly = false;

    KeyCode[] validKeys =
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
        KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
        KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
        KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
    };

    public void BeginTypingSession()
    {
        wordIndex = 0;
        isTypingActive = true;
        hasFailedEarly = false;
        SetupWord();
    }

    void Update()
    {
        if (!isTypingActive || wordIndex >= wordList.Count) return;
        HandleTypingInput();
    }

    void HandleTypingInput()
    {
        foreach (KeyCode kc in validKeys)
        {
            if (Input.GetKeyDown(kc))
            {
                char typedChar = char.ToUpper(kc.ToString()[0]);

                if (typedChar == '\b')
                {
                    if (inputBuffer.Length > 0)
                        inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
                    continue;
                }

                if (!char.IsLetter(typedChar)) continue;

                // Batasi typing panjang dan early fail
                if (inputBuffer.Length >= currentWord.Length || hasFailedEarly) return;

                char expectedChar = currentWord[inputBuffer.Length];
                inputBuffer += typedChar;

                if (typedChar == expectedChar)
                {
                    PlaySound(typeSound);
                    UpdateTypedVisual();
                }
                else
                {
                    PlaySound(wrongTypeSound);
                    ApplyShakeEffect();

                    if (!hasFailedEarly)
                    {
                        hasFailedEarly = true;
                        StartCoroutine(EarlyFailRoutine());
                        UpdateTypedVisual();
                    }
                }
            }
        }
    }

    void SetupWord()
    {
        if (wordIndex >= wordList.Count)
        {
            EndTypingSession();
            return;
        }

        currentWord = wordList[wordIndex];
        //Debug.Log("🆕 Typing word: " + currentWord);
        inputBuffer = "";
        hasFailedEarly = false;

        bool isNowPlayerTarget = wordIndex % 2 == 0;

        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        shadowText.gameObject.SetActive(true);
        typedText.gameObject.SetActive(true);
        (isNowPlayerTarget ? shadowText_Enemy : shadowText_Player).gameObject.SetActive(false);
        (isNowPlayerTarget ? typedText_Enemy : typedText_Player).gameObject.SetActive(false);

        shadowText.text = currentWord;
        typedText.text = "";

        shadowText.fontSize = baseFontSize;
        typedText.fontSize = baseFontSize;

        shadowText.color = shadowTextColor;
        shadowText.alpha = shadowTextColor.a;
        typedText.alpha = 1f;

        shadowText.transform.localScale = Vector3.one;
        typedText.transform.localScale = Vector3.one;

        // 🔁 Mulai timer kata baru
        if (typingTimerUI != null)
        {
            typingTimerUI.OnTimerTimeout = OnWordTimeOut;
            typingTimerUI.StartTimer();
        }
    }

    System.Action<int> onCompleteCallback;
    int correctTypedCount;

    public void StartTyping(System.Action<int> onComplete)
    {
        onCompleteCallback = onComplete;
        correctTypedCount = 0;

        LoadWords(5); // pull 5 words from the glossary
        BeginTypingSession();
    }

    public void LoadWords(int count)
    {
        wordList.Clear();

        if (glossary == null || glossary.words.Count == 0)
        {
            Debug.LogWarning("No Glossary");
            return;
        }

        List<string> copy = new List<string>(glossary.words);

        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int index = Random.Range(0, copy.Count);
            wordList.Add(copy[index].ToUpper()); //uppercase typing
            copy.RemoveAt(index);
        }

        Debug.Log("✅ Loaded words: " + string.Join(", ", wordList));
    }

    void UpdateTypedVisual()
    {
        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;
        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;

        string renderedTyped = "";
        int correctCount = 0;
        bool hasMistake = false;

        for (int i = 0; i < inputBuffer.Length; i++)
        {
            char typedChar = inputBuffer[i];
            char correctChar = currentWord[i];

            if (typedChar == correctChar)
            {
                correctCount++;
                renderedTyped += typedChar;
            }
            else
            {
                hasMistake = true;
                renderedTyped += $"<color=#FF4444>{typedChar}</color>";
            }
        }

        typedText.text = renderedTyped;

        if (useGradientForTypedText)
            typedText.colorGradient = typedTextGradient;

        float newSize = baseFontSize + (correctCount * sizeIncreasePerLetter);
        typedText.fontSize = newSize;
        shadowText.fontSize = newSize;

        if (!hasMistake && inputBuffer.Length == currentWord.Length && !hasFailedEarly)
        {
            StartCoroutine(WordCompleteRoutine());
        }
        else if (!hasMistake && inputBuffer.Length < currentWord.Length && !hasFailedEarly)
        {
            ShakeCameraOnType();
            if (ParticlePoolManager.instance == null) { return;}
            /*typedText.ForceMeshUpdate();
            string theText = typedText.text;
            if (theText == "") return;
            TMP_TextInfo textInfo = typedText.textInfo;

            int matIndex = textInfo.characterInfo[theText.Length - 1].materialReferenceIndex;
            int vertIndex = textInfo.characterInfo[theText.Length - 1].vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[matIndex].vertices;

            Vector3 localMidPos = (vertices[vertIndex + 0] + vertices[vertIndex + 2]) / 2f;

            Vector3 worldPos = typedText.transform.TransformPoint(localMidPos);

            //Debug.Log(worldPos);

            ParticleSpawnData datas = new ParticleSpawnData(null, worldPos, Vector3.zero, Vector3.one * 0.1f, ParticleEnum.OnTextTyped, true);

            if (inputBuffer.Length == currentWord.Length) return;
            ParticlePoolManager.instance.ActivateParticleFX(datas);*/
        }
    }

    void ShakeCameraOnType()
    {
        float shakeOffset = ((float)correctTypedCount / (float)correctTypedCount) / 200f;
        shakeOffset = Mathf.Clamp(shakeOffset, 0, 0.2f);
        CameraShakeManager.instance.ActivateCamShake(new Vector3(0f + shakeOffset, 0f, 0f), 0.15f, 0.05f + shakeOffset);
    }

    IEnumerator WordCompleteRoutine()
    {
        isTypingActive = false;
        typingTimerUI?.StopTimer(); // ⏹ Stop timer

        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        if (wordCompleteParticle)
        {
            GameObject p = Instantiate(wordCompleteParticle, shadowText.transform.position, Quaternion.identity, shadowText.transform.parent);
            p.transform.SetAsLastSibling();
            var ps = p.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        Sequence bounceSeq = DOTween.Sequence();
        bounceSeq.Append(shadowText.transform.DOScale(bounceScale, bounceDuration).SetEase(Ease.OutBack));
        bounceSeq.Join(typedText.transform.DOScale(bounceScale, bounceDuration).SetEase(Ease.OutBack));
        bounceSeq.Append(shadowText.transform.DOScale(1f, fadeOutDuration));
        bounceSeq.Join(typedText.transform.DOScale(1f, fadeOutDuration));

        shadowText.DOFade(0f, fadeOutDuration).SetDelay(bounceDuration);
        typedText.DOFade(0f, fadeOutDuration).SetDelay(bounceDuration);

        yield return new WaitForSeconds(bounceDuration + fadeOutDuration + 0.1f);

        correctTypedCount++;
        wordIndex++;

        if (wordIndex < wordList.Count)
        {
            isTypingActive = true;
            SetupWord();
        }
        else
        {
            EndTypingSession();
        }
    }

    IEnumerator EarlyFailRoutine()
    {
        CameraShakeManager.instance.ActivateCamShake(new Vector3(0f, 1f, 0f), 0.7f, 1f);
        isTypingActive = false;
        typingTimerUI?.StopTimer(); // ⏹ Stop timer

        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        yield return new WaitForSeconds(earlyFailDelay);

        shadowText.DOFade(0f, fadeOutDuration);
        typedText.DOFade(0f, fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration + 0.1f);

        EndTypingSession(); // ❌ Waktu habis → lanjut giliran musuh
    }

    void OnWordTimeOut()
    {
        Debug.Log("⏰ Waktu habis! Langsung ke giliran musuh.");
        hasFailedEarly = true;
        StartCoroutine(EarlyFailRoutine());
    }

    void EndTypingSession()
    {
        Debug.Log("➡ Semua kata selesai diketik! Sekarang giliran musuh!");
        shadowText_Player.gameObject.SetActive(false);
        typedText_Player.gameObject.SetActive(false);
        shadowText_Enemy.gameObject.SetActive(false);
        typedText_Enemy.gameObject.SetActive(false);

        typingTimerUI?.StopTimer(); // ⏹ Pastikan timer mati

        FindFirstObjectByType<BattleUIManager>()?.OnTypingSessionComplete();
        //Debug.Log("Find!");
        onCompleteCallback?.Invoke(correctTypedCount);
    }

    void ApplyShakeEffect()
    {
        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        typedText.transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0f, 0f), 10, 90, false, true);
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
