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
    [SerializeField] AnimationCurve doneWordVignetteAnimCurve;
    [SerializeField] List<ActiveWritingFX> activeWritingFX = new List<ActiveWritingFX>();

    [Header("Colors")]
    public bool useGradientForTypedText = false;
    public VertexGradient typedTextGradient;
    public Color shadowTextColor = new Color(1f, 1f, 1f, 22f / 255f);
    [SerializeField] Color correctCharacterColor;
    [SerializeField] Color correctWordVignetteColor;
    [SerializeField] Color wrongWordVignetteColor;

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
    private string inputBuffer = ""; // Menyimpan input asli pemain
    private bool isTypingActive = false;
    private bool hasFailedEarly = false;
    private Gradient currentColorGradient;
    private int lastActiveFXListLength;
    private int numFXDone;
    private Coroutine CO_StartWritingFX;
    private Coroutine CO_StartCorrectWordVignette;

    private System.Action<int> onCompleteCallback;
    private int correctTypedCount;

    // Daftar tombol keyboard yang valid untuk dideteksi
    KeyCode[] validKeys =
    {
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G,
        KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N,
        KeyCode.O, KeyCode.P, KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U,
        KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z
    };

    #region Initialization & Game Loop

    public void BeginTypingSession()
    {
        wordIndex = 0;
        isTypingActive = true;
        hasFailedEarly = false;
        SetupWord();
    }

    public void StartTyping(System.Action<int> onComplete)
    {
        onCompleteCallback = onComplete;
        correctTypedCount = 0;

        LoadWords(5); // Ambil 5 kata dari glossary
        BeginTypingSession();
    }

    void Update()
    {
        if (!isTypingActive || wordIndex >= wordList.Count) return;
        HandleTypingInput();
    }

    #endregion

    #region Logic

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
            wordList.Add(copy[index].ToUpper()); // Pastikan uppercase
            copy.RemoveAt(index);
        }

        Debug.Log("✅ Loaded words: " + string.Join(", ", wordList));
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
        inputBuffer = "";
        hasFailedEarly = false;

        bool isNowPlayerTarget = wordIndex % 2 == 0;

        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        // Reset UI visibility
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

        // Mulai timer kata baru
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

        if (glossary == null || difficultyProfile == null || performanceManager == null) { Debug.LogError("no glossary!"); return;}
    }

    void HandleTypingInput()
    {
        foreach (KeyCode kc in validKeys)
        {
            if (Input.GetKeyDown(kc))
            {
                char typedChar = char.ToUpper(kc.ToString()[0]);

                // Handle Backspace (Opsional, jika game mengizinkan koreksi)
                if (typedChar == '\b')
                {
                    if (inputBuffer.Length > 0)
                        inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
                    continue;
                }

                if (!char.IsLetter(typedChar)) continue;

                // Batasi panjang typing agar tidak melebihi kata target & cegah input jika sudah gagal
                if (inputBuffer.Length >= currentWord.Length || hasFailedEarly) return;

                char expectedChar = currentWord[inputBuffer.Length];
                inputBuffer += typedChar; // Simpan apa yang diketik player ke buffer

                if (typedChar == expectedChar)
                {
                    PlaySound(typeSound);
                    UpdateTypedVisual();
                }
                else
                {
                    PlaySound(wrongTypeSound);
                    ApplyShakeEffect();

                    // Cek apakah ini kesalahan pertama yang memicu kegagalan
                    if (!hasFailedEarly)
                    {
                        hasFailedEarly = true;
                        StartCoroutine(EarlyFailRoutine());
                        UpdateTypedVisual(); // Update visual untuk menampilkan huruf salah (X)
                    }
                }
            }
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
                correctCount++;
                renderedTyped += typedChar;
            }
            else
            {
                hasMistake = true;
                // --- PERUBAHAN DISINI ---
                // Jika salah ketik, tampilkan 'X' kapital berwarna merah
                renderedTyped += $"<color=#FF4444>X</color>";
                typedText.text = renderedTyped;
            }
        }

        //typedText.text = renderedTyped;

        if (useGradientForTypedText)
            typedText.colorGradient = typedTextGradient;

        // Logika Sukses atau Efek Visual Ketik
        if (!hasMistake && inputBuffer.Length == currentWord.Length && !hasFailedEarly)
        {
            StartCoroutine(WordCompleteRoutine());
        }
        else if (!hasMistake && inputBuffer.Length < currentWord.Length && !hasFailedEarly)
        {
            ShakeCameraOnType();
            typedText.ForceMeshUpdate();
            AddActiveWritingFX(shadowText, inputBuffer.Length - 1);
        }
    }
    

    #endregion

    #region Visual Effects & Animation

    private void AddActiveWritingFX(TMP_Text txt, int charIndex)
    {
        activeWritingFX.Add(new ActiveWritingFX(txt, charIndex, resizeAnimationCurve, correctCharacterColor));
        if (CO_StartWritingFX != null) return;
        CO_StartWritingFX = StartCoroutine(StartWritingFX());
    }

    IEnumerator StartWritingFX()
    {
        float waitTimer = 0f;
        float maxWaitTime = 2f;
        if (activeWritingFX.Count < 1) { CO_StartWritingFX = null; yield break; }

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
        }
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
    }

    void ShakeCameraOnType()
    {
        float shakeOffset = (((float)correctTypedCount + 1 / (float)wordList.Count)) / 200f;
        shakeOffset = Mathf.Clamp(shakeOffset, 0, 0.2f);
        CameraShakeManager.instance.ActivateCamShake(new Vector3(0f + shakeOffset, 0f, 0f), 0.15f, 0.05f + shakeOffset);
    }

    void ApplyShakeEffect()
    {
        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;
        typedText.transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0f, 0f), 10, 90, false, true);
    }

    void OnDoneWordVignette(Color c)
    {
        if (PostProcessingManager.instance == null) return;
        if (CO_StartCorrectWordVignette != null) return;
        CO_StartCorrectWordVignette = StartCoroutine(StartCorrectWordVignette(c));
        PostProcessingManager.instance.ActivateVignette(true);
    }

    IEnumerator StartCorrectWordVignette(Color c)
    {
        float timer = 0f;
        float maxTimer = doneWordVignetteAnimCurve[doneWordVignetteAnimCurve.length - 1].time;

        while (timer <= maxTimer)
        {
            float flt_t = timer / maxTimer;
            float flt_Eval = doneWordVignetteAnimCurve.Evaluate(flt_t);
            PostProcessingManager.instance.SetVignette(flt_Eval);
            PostProcessingManager.instance.SetVignetteColor(c);
            timer += Time.deltaTime;
            yield return null;
        }
        PostProcessingManager.instance.ActivateVignette(false);
        CO_StartCorrectWordVignette = null;
    }

    #endregion

    #region Game Flow Routines

    IEnumerator WordCompleteRoutine()
    {
        isTypingActive = false;
        typingTimerUI?.StopTimer();

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
        wordIndex++;

        // Update Counter UI with Gradient
        int inverseWordIndex = 0;
        if (wordIndex >= 1) { inverseWordIndex = wordList.Count - wordIndex; }
        int gradientVal = Mathf.Clamp(inverseWordIndex, 1, wordList.Count);
        float t = Mathf.InverseLerp(0, wordList.Count, gradientVal);

        if (currentColorGradient == null)
            counterUIController.UpdateTextCounter(correctTypedCount, Color.white);
        else
        {
            Color c = currentColorGradient.Evaluate(t);
            counterUIController.UpdateTextCounter(correctTypedCount, c);
        }

        if (AudioPoolManager.instance != null) { AudioPoolManager.instance.RequestPlayAudio(new AudioSpawnData(wordDoneSuccess, false, Vector3.zero)); }
        OnDoneWordVignette(correctWordVignetteColor);

        shadowText.color = correctCharacterColor;
        Sequence bounceSeq = DOTween.Sequence();
        bounceSeq.Append(shadowText.transform.DOScale(bounceScale, bounceDuration).SetEase(Ease.OutBack));
        bounceSeq.Join(typedText.transform.DOScale(bounceScale, bounceDuration).SetEase(Ease.OutBack));
        bounceSeq.Append(shadowText.transform.DOScale(1f, fadeOutDuration));
        bounceSeq.Join(typedText.transform.DOScale(1f, fadeOutDuration));

        shadowText.DOFade(0f, fadeOutDuration).SetDelay(bounceDuration);
        typedText.DOFade(0f, fadeOutDuration).SetDelay(bounceDuration);

        yield return new WaitForSeconds(bounceDuration + fadeOutDuration + 0.1f);

        performanceManager.RegisterWordResult(true, typingTimerUI != null ? typingTimerUI.ElapsedTime : 0f, currentWord.Length);
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
        OnDoneWordVignette(wrongWordVignetteColor);
        isTypingActive = false;
        typingTimerUI?.StopTimer();

        bool isNowPlayerTarget = wordIndex % 2 == 0;
        TMP_Text shadowText = isNowPlayerTarget ? shadowText_Player : shadowText_Enemy;
        TMP_Text typedText = isNowPlayerTarget ? typedText_Player : typedText_Enemy;

        shadowText.color = shadowTextColor;
        shadowText.alpha = shadowTextColor.a;

        yield return new WaitForSeconds(earlyFailDelay);

        shadowText.DOFade(0f, fadeOutDuration);
        typedText.DOFade(0f, fadeOutDuration);

        yield return new WaitForSeconds(fadeOutDuration + 0.1f);

        performanceManager.RegisterWordResult(false, typingTimerUI != null ? typingTimerUI.ElapsedTime : 0f, currentWord.Length);
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
        counterUIController.TextCenterReposition();
        shadowText_Player.gameObject.SetActive(false);
        typedText_Player.gameObject.SetActive(false);
        shadowText_Enemy.gameObject.SetActive(false);
        typedText_Enemy.gameObject.SetActive(false);

        typingTimerUI?.StopTimer();

        FindFirstObjectByType<BattleUIManager>()?.OnTypingSessionComplete();
        onCompleteCallback?.Invoke(correctTypedCount);
    }

    #endregion

    #region Utilities

    public void AddColorGradient(Gradient newGradient)
    {
        currentColorGradient = newGradient;
    }

    public void EmptyCounterText()
    {
        counterUIController.EmptyText();
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    #endregion
}