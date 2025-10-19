using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using ParticleData.SpawnData;
using System;
using AudioData;

public class TypewritingManager : MonoBehaviour
{
    [Header("Word Source")]
    [SerializeField] WordGlossary glossary;
    [SerializeField] DifficultyProfile difficultyProfile;
    [SerializeField] PerformanceManager performanceManager;
    public List<string> wordList;

    [Header("UI References")]
    public TMP_Text shadowText_Player;
    public TMP_Text typedText_Player;
    public TMP_Text shadowText_Enemy;
    public TMP_Text typedText_Enemy;
    [SerializeField] TMP_Text counterText;
    [SerializeField] TextCounterUIController counterUIController;

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

    [SerializeField] AnimationCurve colorTransitionCurve;
    [SerializeField] Gradient colorTransition;
    [SerializeField] AnimationCurve resizeAnimationCurve;
    [SerializeField] AnimationCurve correctWordVignetteAnimCurve;
    [SerializeField] List<ActiveWritingFX> activeWritingFX = new List<ActiveWritingFX>();

    [Header("Colors")]
    public bool useGradientForTypedText = false;
    public VertexGradient typedTextGradient;
    /*typedTextGradient note
    FDD776
    FFD600
    563921
    331A00*/
    public Color shadowTextColor = new Color(1f, 1f, 1f, 22f / 255f);
    [SerializeField] Color correctCharacterColor;
    [SerializeField] Color correctWordVignetteColor;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip typeSound;
    public AudioClip wrongTypeSound;
    [SerializeField] AudioClip wordDoneSuccess;

    [Header("Early Fail Settings")]
    public float earlyFailDelay = 2f;

    [Header("Timer Integration")]
    public TypingWordTimer typingTimerUI;

    private int wordIndex = 0;
    private string currentWord;
    private string inputBuffer = "";
    private bool isTypingActive = false;
    private bool hasFailedEarly = false;
    private Gradient currentColorGradient;

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

        lastActiveFXListLength = 0;
        numFXDone = 0;
        activeWritingFX.Clear();

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
    int currentWordIndex;

    public void StartTyping(System.Action<int> onComplete)
    {
        onCompleteCallback = onComplete;
        correctTypedCount = 0;
        currentWordIndex = 0;

        LoadWords(); // pull 5 words from the glossary
        BeginTypingSession();
    }

    public void LoadWords()
    {
        wordList.Clear();

        if (glossary == null || difficultyProfile == null || performanceManager == null)
        {
            Debug.LogWarning("No Glossary");
            return;
        }
        
        // Get the appropriate tier based on the player's performance score
        string performanceTier = performanceManager.GetPerformanceTier();
        float score = performanceManager.performanceScore;
        DifficultyRatio profile = Array.Find(difficultyProfile.difficultyProfiles, p => p.profileName == performanceTier); //difficultyProfile.GetProfile(score);

        if (profile == null)
        {
            Debug.LogWarning($"Difficulty profile not found!");
            return;
        }

        // Load words based on the profile's ratios
        wordList.AddRange(glossary.GetRandomWords(Difficulty.Easy, profile.easyCount));
        wordList.AddRange(glossary.GetRandomWords(Difficulty.Medium, profile.mediumCount));
        wordList.AddRange(glossary.GetRandomWords(Difficulty.Hard, profile.hardCount));
        
        for (int i = 0; i < wordList.Count; i++)
        {
            wordList[i] = wordList[i].ToUpper(); //uppercase typing
        }

        Debug.Log($"✅ Loaded words for tier '{performanceTier}' (Score: {score}): {string.Join(", ", wordList)}");
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
                //shadowText.ForceMeshUpdate();
                correctCount++;
                renderedTyped += typedChar;
                //OnCorrectType(i);
            }
            else
            {
                hasMistake = true;
                renderedTyped += $"<color=#FF4444>{typedChar}</color>";
                typedText.text = renderedTyped;
            }
        }

        //typedText.text = renderedTyped;

        if (useGradientForTypedText)
            typedText.colorGradient = typedTextGradient;

        float newSize = baseFontSize + (correctCount * sizeIncreasePerLetter);
        //typedText.fontSize = newSize;
        //shadowText.fontSize = newSize;

        if (!hasMistake && inputBuffer.Length == currentWord.Length && !hasFailedEarly)
        {
            StartCoroutine(WordCompleteRoutine());
        }
        else if (!hasMistake && inputBuffer.Length < currentWord.Length && !hasFailedEarly)
        {
            ShakeCameraOnType();
            typedText.ForceMeshUpdate();
            //AddActiveTypewriteEffect(typedText);
            AddActiveWritingFX(shadowText, inputBuffer.Length - 1);
            //StartCoroutine(DoTextScaleBounce(typedText));
            if (ParticlePoolManager.instance == null) { return; }
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


    private void AddActiveWritingFX(TMP_Text txt, int charIndex)
    {
        activeWritingFX.Add(new ActiveWritingFX(txt, charIndex, resizeAnimationCurve, correctCharacterColor));
        //Debug.LogError("Added new FX!");
        if (CO_StartWritingFX != null) return;
        CO_StartWritingFX = StartCoroutine(StartWritingFX());
    }

    int lastActiveFXListLength;
    int numFXDone;
    Coroutine CO_StartWritingFX;
    IEnumerator StartWritingFX()
    {
        float waitTimer = 0f;
        float maxWaitTime = 2f;
        if (activeWritingFX.Count < 1) { CO_StartWritingFX = null; Debug.LogError("List empty!"); yield break; }
        while (true)
        {
            if (lastActiveFXListLength == activeWritingFX.Count && lastActiveFXListLength == numFXDone)
            {
                if (waitTimer <= maxWaitTime)
                {
                    waitTimer += Time.deltaTime;
                }
                else
                {
                    CO_StartWritingFX = null;
                    //Debug.LogError("We stopped...");
                    yield break;
                }
            }
            else
            {
                waitTimer = 0f;
                lastActiveFXListLength = activeWritingFX.Count;
                for (int i = 0; i < activeWritingFX.Count; i++)
                {
                    ActiveWritingFX index = activeWritingFX[i];
                    if (index == null) { activeWritingFX[i] = null; activeWritingFX.Remove(index); }
                    ChangeColorWrittenColor(index);

                    if (index.timer <= index.duration)
                    {
                        index.timer += Time.deltaTime;
                        ScaleBounceWrittenCharacter(index);
                        if (index.timer > index.duration) { numFXDone++; }
                    }
                    else { continue; }
                    index.txt.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                }
            }
            
            yield return null;
        }
    }

    private void ChangeColorWrittenColor(ActiveWritingFX text)
    {
        Color32[] vertexColors = text.GetTextInfo().meshInfo[text.GetMeshIndex()].colors32;
        for (int i = 0; i < 4; i++)
        {
            vertexColors[text.GetVertexIndex() + i] = correctCharacterColor;
            //Debug.LogError(vertexColors[text.GetVertexIndex() + i]);
        }
        //Debug.LogError("Color changed!");
    }

    private void ScaleBounceWrittenCharacter(ActiveWritingFX text)
    {
        var flt_t = Mathf.Clamp01(text.timer / text.duration);
        var flt_Scale = text.animCurve.Evaluate(flt_t);
        if (!text.GetCharacterInfo().isVisible) return;
        Vector3 vert0 = text.baseVerts[0];
        Vector3 vert1 = text.baseVerts[2];
        Vector3 center = (vert0 + vert1) / 2f;
        Vector3[] verts = text.GetTextInfo().meshInfo[text.GetMeshIndex()].vertices;

        for (int k = 0; k < 4; k++)
        {
            Vector3 offSet = text.baseVerts[k] - center;
            verts[text.GetVertexIndex() + k] = center + offSet * flt_Scale;
        }
        //Debug.LogError("Animated bounce!");
    }

    public void AddColorGradient(Gradient newGradient)
    {
        currentColorGradient = newGradient;
    }

    public void EmptyCounterText()
    {
        counterUIController.EmptyText();
    }

    void ShakeCameraOnType()
    {
        float shakeOffset = (((float)correctTypedCount + 1 / (float)wordList.Count)) / 200f;
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

        correctTypedCount++;
        /*counterText.gameObject.SetActive(true);
        counterText.text = correctTypedCount.ToString();*/
        wordIndex++;
        int inverseWordIndex = 0;
        if (wordIndex >= 1) { inverseWordIndex = wordList.Count - wordIndex; }
        int gradientVal = 0;
        gradientVal = Mathf.Clamp(inverseWordIndex, 1, wordList.Count);
        float t = Mathf.InverseLerp(0, wordList.Count, gradientVal);

        if (currentColorGradient == null)
        {
            counterUIController.UpdateTextCounter(correctTypedCount, Color.white);
        }
        else
        {
            Color c = currentColorGradient.Evaluate(t);
            counterUIController.UpdateTextCounter(correctTypedCount, c);
        }

        if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(wordDoneSuccess, false, Vector3.zero)); }
        OnCorrectWordVignette();

        shadowText.color = correctCharacterColor;
        Sequence bounceSeq = DOTween.Sequence();
        bounceSeq.Append(shadowText.transform.DOScale(bounceScale, bounceDuration).SetEase(Ease.OutBack));
        bounceSeq.Join(typedText.transform.DOScale(bounceScale, bounceDuration).SetEase(Ease.OutBack));
        bounceSeq.Append(shadowText.transform.DOScale(1f, fadeOutDuration));
        bounceSeq.Join(typedText.transform.DOScale(1f, fadeOutDuration));

        shadowText.DOFade(0f, fadeOutDuration).SetDelay(bounceDuration);
        typedText.DOFade(0f, fadeOutDuration).SetDelay(bounceDuration);

        yield return new WaitForSeconds(bounceDuration + fadeOutDuration + 0.1f);

        performanceManager.RegisterWordResult(true, typingTimerUI != null ? typingTimerUI.ElapsedTime : 0f);
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

    void OnCorrectWordVignette()
    {
        if (PostProcessingManager.instance == null) return;
        if (CO_StartCorrectWordVignette != null) return;
        CO_StartCorrectWordVignette = StartCoroutine(StartCorrectWordVignette());
        PostProcessingManager.instance.ActivateVignette(true);
        
    }

    Coroutine CO_StartCorrectWordVignette;
    IEnumerator StartCorrectWordVignette()
    {
        float timer = 0f;
        float maxTimer = correctWordVignetteAnimCurve[correctWordVignetteAnimCurve.length - 1].time;

        while (timer <= maxTimer)
        {
            float flt_t = timer / maxTimer;
            float flt_Eval = correctWordVignetteAnimCurve.Evaluate(flt_t);
            PostProcessingManager.instance.SetVignette(flt_Eval);
            PostProcessingManager.instance.SetVignetteColor(correctWordVignetteColor);
            timer += Time.deltaTime;
            //Debug.LogError(flt_t);
            yield return null;
        }
        PostProcessingManager.instance.ActivateVignette(false);
        CO_StartCorrectWordVignette = null;
    }

    IEnumerator EarlyFailRoutine()
    {
        CameraShakeManager.instance.ActivateCamShake(new Vector3(0f, 1f, 0f), 0.7f, 1f);
        isTypingActive = false;
        typingTimerUI?.StopTimer(); // ⏹ Stop timer

        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        shadowText.color = shadowTextColor;
        shadowText.alpha = shadowTextColor.a;

        yield return new WaitForSeconds(earlyFailDelay);

        shadowText.DOFade(0f, fadeOutDuration);
        typedText.DOFade(0f, fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration + 0.1f);

        performanceManager.RegisterWordResult(false, typingTimerUI != null ? typingTimerUI.ElapsedTime : 0f);
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
        //counterText.gameObject.SetActive(false);
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
