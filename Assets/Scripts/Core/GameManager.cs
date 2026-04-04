using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 게임 전반의 흐름을 관리하는 매니저 (싱글턴).
/// BattleManager.OnAnyBaseDestroyed 이벤트를 수신해 게임 오버 화면을 표시한다.
/// R키로 게임을 재시작한다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─── 게임 오버 상태 ────────────────────────────────────────────────
    private bool   isGameOver    = false;
    private bool   playerWon     = false;
    private string resultMessage = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        BattleManager.OnAnyBaseDestroyed += OnBaseDestroyed;
    }

    private void OnDisable()
    {
        BattleManager.OnAnyBaseDestroyed -= OnBaseDestroyed;
    }

    private void Update()
    {
        if (isGameOver && Keyboard.current != null && Keyboard.current[Key.R].wasPressedThisFrame)
            RestartGame();
    }

    /// <summary>
    /// 기지 파괴 수신.
    /// team 0 = 아군 기지 파괴 → 패배 / team 1 = 적군 기지 파괴 → 승리.
    /// </summary>
    private void OnBaseDestroyed(int team)
    {
        if (isGameOver) return;

        isGameOver    = true;
        playerWon     = (team == 1);
        resultMessage = playerWon ? "★ 승리! ★" : "✕ 패배 ✕";

        Debug.Log($"[GameManager] 게임 오버: {resultMessage} (team={team})");
    }

    private void RestartGame()
    {
        isGameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        if (!isGameOver) return;

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float boxW = 420f;
        float boxH = 160f;
        float boxX = (Screen.width  - boxW) * 0.5f;
        float boxY = (Screen.height - boxH) * 0.5f;

        GUI.color = playerWon
            ? new Color(0.1f, 0.2f, 0.5f, 0.95f)
            : new Color(0.5f, 0.1f, 0.1f, 0.95f);
        GUI.DrawTexture(new Rect(boxX, boxY, boxW, boxH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 48,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = new Color(0.85f, 0.85f, 0.85f, 1f) }
        };

        GUI.Label(new Rect(boxX, boxY + 10f,  boxW, 70f), resultMessage, titleStyle);
        GUI.Label(new Rect(boxX, boxY + 100f, boxW, 30f), "[R] 재시작",  subStyle);
    }
}
