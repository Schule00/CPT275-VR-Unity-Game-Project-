using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class SlotMachineUI : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STATIC SCORE API — readable from ANY other script in the project
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>The player's current score. Read this from any script.</summary>
    public static int CurrentScore { get; private set; }

    /// <summary>How many spins the player has used.</summary>
    public static int SpinsUsed { get; private set; }

    /// <summary>True once all 10 spins are spent.</summary>
    public static bool GameOver { get; private set; }

    /// <summary>
    /// Fires every time the score changes. Subscribe to this in ScoreBoardManager.
    /// Passes the new score as an int.
    /// </summary>
    public static event System.Action<int> OnScoreChanged;

    /// <summary>Fires once when all spins are used up.</summary>
    public static event System.Action<int> OnGameOver;

    // ── Symbols ────────────────────────────────────────────────────────────────
    private readonly string[] symbols = { "7", "#", "@", "~" };

    private readonly Dictionary<string, int> threePoints = new Dictionary<string, int>
    {
        { "7", 100 }, { "#", 50 }, { "@", 25 }, { "~", 10 }
    };

    private const int PAIR_POINTS = 5;
    private const int MAX_SPINS = 10;

    // ── Inspector ──────────────────────────────────────────────────────────────
    [Header("Spin")]
    public float spinDuration = 2.0f;
    public float reelStagger = 0.35f;
    public float symbolsPerSec = 12f;

    [Header("Score Counter")]
    [Tooltip("How fast the score display counts up to the new value (points per second)")]
    public float scoreCountSpeed = 80f;

    [Header("Idle Prompt")]
    public float idleTimeout = 15f;

    // ── UI refs ────────────────────────────────────────────────────────────────
    private Text[] reelTexts = new Text[3];
    private Image[] reelPanels = new Image[3];
    private Text resultText;
    private Text scoreText;
    private Text spinsText;
    private Image[] spinPips;
    private GameObject pullMeRoot;
    private Text pullMeText;
    private GameObject gameOverRoot;
    private Text finalScoreText;

    // ── Private state ──────────────────────────────────────────────────────────
    private bool isSpinning;
    private bool hasEverSpun;
    private float idleTimer;
    private string[] finalSymbols = new string[3];
    private float canvasH;

    // Score display counter (animates up to CurrentScore smoothly)
    private float displayScore;

    // ── Unity ──────────────────────────────────────────────────────────────────
    void Awake()
    {
        // Reset static state (important if scene reloads)
        CurrentScore = 0;
        SpinsUsed = 0;
        GameOver = false;
        displayScore = 0f;

        RectTransform cr = GetComponent<RectTransform>();
        canvasH = cr.rect.height;
        if (canvasH <= 0) canvasH = 600f;

        CanvasScaler cs = GetComponent<CanvasScaler>();
        if (cs != null) cs.enabled = false;

        BuildUI();
        ShowPullMe(true);
        RefreshHUD();
    }

    void Update()
    {
        // Animate the score display counting up
        if (displayScore < CurrentScore)
        {
            displayScore = Mathf.MoveTowards(displayScore, CurrentScore,
                                             scoreCountSpeed * Time.deltaTime);
            if (scoreText != null)
                scoreText.text = $"SCORE: {Mathf.FloorToInt(displayScore)}";
        }

        if (isSpinning || !hasEverSpun) return;
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTimeout && !GameOver)
            ShowPullMe(true);
        AnimatePullMe();
    }

    // ── Public entry point called by LeverController ───────────────────────────
    public void TriggerSpin()
    {
        if (isSpinning || GameOver) return;

        ShowPullMe(false);
        idleTimer = 0f;
        hasEverSpun = true;
        resultText.text = "";

        SpinsUsed++;
        RefreshHUD();

        for (int i = 0; i < 3; i++)
            finalSymbols[i] = symbols[Random.Range(0, symbols.Length)];

        StartCoroutine(SpinAll());
    }

    // ── Add points and notify listeners ───────────────────────────────────────
    void AddScore(int points)
    {
        CurrentScore += points;
        OnScoreChanged?.Invoke(CurrentScore);   // ← ScoreBoardManager gets this
        Debug.Log($"[SlotMachine] Score: {CurrentScore}  (+" + points + ")");
    }

    // ── Spin coroutines ────────────────────────────────────────────────────────
    IEnumerator SpinAll()
    {
        isSpinning = true;
        bool[] done = new bool[3];
        for (int i = 0; i < 3; i++)
            StartCoroutine(SpinReel(i, spinDuration + reelStagger * i, done));
        yield return new WaitUntil(() => done[0] && done[1] && done[2]);
        yield return new WaitForSeconds(0.2f);
        Evaluate();
        isSpinning = false;

        if (SpinsUsed >= MAX_SPINS)
        {
            yield return new WaitForSeconds(1.8f);
            TriggerGameOver();
        }
    }

    IEnumerator SpinReel(int idx, float dur, bool[] done)
    {
        float e = 0f, st = 0f, interval = 1f / symbolsPerSec;
        int s = 0;
        while (e < dur)
        {
            e += Time.deltaTime; st += Time.deltaTime;
            if (st >= interval)
            {
                st = 0f;
                s = (s + 1) % symbols.Length;
                reelTexts[idx].text = symbols[s];
            }
            yield return null;
        }
        reelTexts[idx].text = finalSymbols[idx];
        done[idx] = true;
    }

    // ── Evaluate result ────────────────────────────────────────────────────────
    void Evaluate()
    {
        bool three = finalSymbols[0] == finalSymbols[1] &&
                     finalSymbols[1] == finalSymbols[2];
        bool two = !three && (finalSymbols[0] == finalSymbols[1] ||
                                finalSymbols[1] == finalSymbols[2] ||
                                finalSymbols[0] == finalSymbols[2]);
        if (three)
        {
            int pts = threePoints.ContainsKey(finalSymbols[0]) ? threePoints[finalSymbols[0]] : 10;
            AddScore(pts);
            ShowResult($"JACKPOT!  +{pts} PTS", Color.yellow);
            StartCoroutine(FlashReels(Color.yellow));
        }
        else if (two)
        {
            AddScore(PAIR_POINTS);
            ShowResult($"PAIR!  +{PAIR_POINTS} PTS", new Color(0.4f, 1f, 0.4f));
        }
        else
        {
            ShowResult("NO MATCH", new Color(1f, 0.45f, 0.45f));
        }
    }

    IEnumerator FlashReels(Color flash)
    {
        Color[] orig = {
            new Color(0.18f,0.07f,0.07f),
            new Color(0.07f,0.16f,0.07f),
            new Color(0.07f,0.07f,0.18f)
        };
        for (int n = 0; n < 5; n++)
        {
            foreach (var p in reelPanels) p.color = flash;
            yield return new WaitForSeconds(0.1f);
            for (int i = 0; i < 3; i++) reelPanels[i].color = orig[i];
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ── Game Over ──────────────────────────────────────────────────────────────
    void TriggerGameOver()
    {
        GameOver = true;
        ShowPullMe(false);
        if (gameOverRoot != null) gameOverRoot.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = $"FINAL SCORE\n{CurrentScore} PTS";
        OnGameOver?.Invoke(CurrentScore);   // ← ScoreBoardManager gets this too
        Debug.Log($"[SlotMachine] GAME OVER. Final score: {CurrentScore}");
    }

    // ── HUD ────────────────────────────────────────────────────────────────────
    void RefreshHUD()
    {
        int spinsLeft = MAX_SPINS - SpinsUsed;
        if (spinsText != null) spinsText.text = $"SPINS: {spinsLeft} LEFT";
        if (spinPips != null)
            for (int i = 0; i < spinPips.Length; i++)
                spinPips[i].color = i < spinsLeft ? Gold() : new Color(0.22f, 0.22f, 0.22f);
    }

    void ShowPullMe(bool show)
    {
        if (pullMeRoot != null) pullMeRoot.SetActive(show);
    }

    void AnimatePullMe()
    {
        if (pullMeRoot == null || !pullMeRoot.activeSelf) return;
        float t = Mathf.Abs(Mathf.Sin(Time.time * 2.5f));
        pullMeText.color = Color.Lerp(Color.yellow, Color.red, t);
        pullMeText.transform.localScale = Vector3.one * (1f + 0.07f * t);
    }

    void ShowResult(string msg, Color col) { resultText.text = msg; resultText.color = col; }
    Color Gold() => new Color(0.9f, 0.75f, 0.1f);
    int FontPct(float p) => Mathf.Max(8, Mathf.RoundToInt(canvasH * p));
    Vector2 V2(float x, float y) => new Vector2(x, y);

    // ── UI Build ───────────────────────────────────────────────────────────────
    void BuildUI()
    {
        RectTransform root = GetComponent<RectTransform>();
        MakePanel("BG", root, new Color(0.06f, 0.06f, 0.10f), V2(0, 0), V2(1, 1));

        // Title band
        RectTransform titleBand = MakePanel("TitleBand", root,
            new Color(0.12f, 0.08f, 0.02f), V2(0, 0.86f), V2(1, 1));
        MakeText("Title", titleBand, "Lucky? Pull?",
            FontPct(0.08f), Color.yellow, FontStyle.Bold, V2(0, 0), V2(0.65f, 1))
            .alignment = TextAnchor.MiddleCenter;
        spinsText = MakeText("Spins", titleBand, "SPINS: 10 LEFT",
            FontPct(0.048f), new Color(1f, 0.85f, 0.3f), FontStyle.Bold, V2(0.65f, 0), V2(1, 1));
        spinsText.alignment = TextAnchor.MiddleCenter;
        MakePanel("TopBorder", root, Gold(), V2(0, 0.855f), V2(1, 0.862f));

        // Pip row
        RectTransform pipBand = MakePanel("PipBand", root,
            new Color(0.07f, 0.05f, 0.01f), V2(0, 0.84f), V2(1, 0.855f));
        spinPips = new Image[MAX_SPINS];
        for (int i = 0; i < MAX_SPINS; i++)
        {
            float x0 = i / (float)MAX_SPINS + 0.005f;
            float x1 = (i + 0.88f) / (float)MAX_SPINS;
            spinPips[i] = MakePanel($"Pip{i}", pipBand, Gold(), V2(x0, 0.1f), V2(x1, 0.9f))
                          .GetComponent<Image>();
        }

        // Reel area
        RectTransform reelArea = MakePanel("ReelArea", root,
            new Color(0.04f, 0.04f, 0.06f), V2(0.04f, 0.40f), V2(0.96f, 0.84f));
        float[] cMin = { 0.02f, 0.36f, 0.69f };
        float[] cMax = { 0.34f, 0.67f, 0.98f };
        Color[] cBG = {
            new Color(0.18f,0.07f,0.07f),
            new Color(0.07f,0.16f,0.07f),
            new Color(0.07f,0.07f,0.18f)
        };
        for (int i = 0; i < 3; i++)
        {
            MakePanel($"CB{i}", reelArea, Gold(),
                V2(cMin[i] - 0.01f, -0.02f), V2(cMax[i] + 0.01f, 1.02f));
            RectTransform col = MakePanel($"Col{i}", reelArea, cBG[i],
                V2(cMin[i], 0.04f), V2(cMax[i], 0.96f));
            reelPanels[i] = col.GetComponent<Image>();
            Text sym = MakeText($"Sym{i}", col, symbols[i],
                FontPct(0.18f), Color.white, FontStyle.Bold, V2(0, 0), V2(1, 1));
            sym.alignment = TextAnchor.MiddleCenter;
            reelTexts[i] = sym;
        }
        MakePanel("Div1", reelArea, Gold(), V2(0.345f, 0), V2(0.355f, 1));
        MakePanel("Div2", reelArea, Gold(), V2(0.675f, 0), V2(0.685f, 1));

        // Result band
        RectTransform resultBand = MakePanel("ResultBand", root,
            new Color(0, 0, 0, 0), V2(0, 0.28f), V2(1, 0.40f));
        resultText = MakeText("Result", resultBand, "",
            FontPct(0.075f), Color.green, FontStyle.Bold, V2(0, 0), V2(1, 1));
        resultText.alignment = TextAnchor.MiddleCenter;

        // PULL ME
        pullMeRoot = new GameObject("PullMeRoot");
        pullMeRoot.transform.SetParent(root, false);
        RectTransform pmRT = pullMeRoot.AddComponent<RectTransform>();
        pmRT.anchorMin = V2(0, 0.28f); pmRT.anchorMax = V2(1, 0.40f);
        pmRT.offsetMin = pmRT.offsetMax = Vector2.zero;
        pullMeText = MakeText("PullMe", pmRT, "★  PULL ME!  ★",
            FontPct(0.085f), Color.yellow, FontStyle.Bold, V2(0, 0), V2(1, 1));
        pullMeText.alignment = TextAnchor.MiddleCenter;
        pullMeRoot.SetActive(false);

        // Bottom band — score left, guide right
        RectTransform btm = MakePanel("Bottom", root,
            new Color(0.08f, 0.05f, 0.02f), V2(0, 0), V2(1, 0.27f));
        MakePanel("BtmBorder", root, Gold(), V2(0, 0.265f), V2(1, 0.272f));

        scoreText = MakeText("Score", btm, "SCORE: 0",
            FontPct(0.075f), Color.yellow, FontStyle.Bold, V2(0, 0.52f), V2(0.48f, 1f));
        scoreText.alignment = TextAnchor.MiddleCenter;

        string guide = "7 7 7 = 100 pts   # # # = 50 pts\n@ @ @ = 25 pts    ~ ~ ~ = 10 pts\nAny Pair = 5 pts";
        MakeText("Guide", btm, guide,
            FontPct(0.037f), new Color(0.78f, 0.78f, 0.58f), FontStyle.Normal,
            V2(0.52f, 0.46f), V2(1f, 1f)).alignment = TextAnchor.MiddleCenter;

        // Game Over overlay
        gameOverRoot = new GameObject("GameOver");
        gameOverRoot.transform.SetParent(root, false);
        RectTransform goRT = gameOverRoot.AddComponent<RectTransform>();
        goRT.anchorMin = V2(0.04f, 0.22f); goRT.anchorMax = V2(0.96f, 0.82f);
        goRT.offsetMin = goRT.offsetMax = Vector2.zero;
        MakePanel("GOBg", goRT, new Color(0.05f, 0.05f, 0.05f, 0.97f), V2(0, 0), V2(1, 1));
        MakePanel("GOBorderT", goRT, Gold(), V2(0, 0.985f), V2(1, 1));
        MakePanel("GOBorderB", goRT, Gold(), V2(0, 0), V2(1, 0.015f));
        finalScoreText = MakeText("FinalScore", goRT, "",
            FontPct(0.095f), Color.yellow, FontStyle.Bold, V2(0, 0.38f), V2(1, 0.95f));
        finalScoreText.alignment = TextAnchor.MiddleCenter;
        MakeText("GOSub", goRT, "GAME OVER  —  No spins remaining",
            FontPct(0.042f), new Color(1f, 0.4f, 0.4f), FontStyle.Italic,
            V2(0, 0.06f), V2(1, 0.36f)).alignment = TextAnchor.MiddleCenter;
        gameOverRoot.SetActive(false);

        // Frame
        MakePanel("FT", root, Gold(), V2(0, 0.995f), V2(1, 1));
        MakePanel("FB", root, Gold(), V2(0, 0), V2(1, 0.005f));
        MakePanel("FL", root, Gold(), V2(0, 0), V2(0.007f, 1));
        MakePanel("FR", root, Gold(), V2(0.993f, 0), V2(1, 1));
    }

    // ── Factory ───────────────────────────────────────────────────────────────
    RectTransform MakePanel(string name, RectTransform parent, Color color,
                            Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return rt;
    }

    Text MakeText(string name, RectTransform parent, string content,
                  int size, Color color, FontStyle style,
                  Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.text = content; t.fontSize = size; t.color = color; t.fontStyle = style;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return t;
    }
}