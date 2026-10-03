using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Jugador")]
    public PlayerMovement player;
    public Transform startPoint;

    [Header("Reglas de caída")]
    public bool reloadSceneIfNoCheckpoint = true;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text collectiblesText;
    public GameObject winPanel;
    public TMP_Text winSummaryText;

    [Header("Progreso")]
    public int totalCollectibles = 0;

    int score;
    int collected;
    int optionalCollected;
    int optionalTotal;
    Vector3 respawnPos;
    bool hasCheckpoint;
    bool won;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        respawnPos = startPoint != null ? startPoint.position : player.transform.position;

        Collectible[] all = FindObjectsByType<Collectible>(FindObjectsSortMode.None);
        if (totalCollectibles <= 0) totalCollectibles = all.Length;
        foreach (Collectible c in all) if (c.isOptional) optionalTotal++;

        if (winPanel != null) winPanel.SetActive(false);
        UpdateUI();
    }

    void Update()
    {
        // Atajo: con Enter también pasa de nivel cuando se ganó
        if (won && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            LoadNextLevel();
    }

    public void AddCollectible(int points, bool isOptional)
    {
        score += points;
        collected++;
        if (isOptional) optionalCollected++;
        UpdateUI();
    }

    public void SetCheckpoint(Vector3 pos)
    {
        respawnPos = pos;
        hasCheckpoint = true;
    }

    public void OnPlayerFell()
    {
        if (!hasCheckpoint && reloadSceneIfNoCheckpoint)
        {
            RestartLevel();
            return;
        }
        RespawnPlayer();
    }

    public void RespawnPlayer()
    {
        player.ResetMotion();
        player.transform.position = respawnPos;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ShowWin()
    {
        if (won) return;
        won = true;

        if (winPanel != null) winPanel.SetActive(true);
        if (winSummaryText != null)
            winSummaryText.text = $"¡Nivel completado!\nPuntos: {score}\nColeccionables: {collected}/{totalCollectibles}" +
                                  (optionalTotal > 0 ? $"\nOpcionales: {optionalCollected}/{optionalTotal}" : "");
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings) next = 0;
        SceneManager.LoadScene(next);
    }

    // Botón de respaldo dibujado por código: aparece siempre al ganar
    void OnGUI()
    {
        if (!won) return;

        float w = 280f, h = 70f;
        float top = Screen.height * 0.35f;
        GUI.Box(new Rect((Screen.width - w) / 2f - 20f, top, w + 40f, 200f), "¡Nivel completado!");

        if (GUI.Button(new Rect((Screen.width - w) / 2f, top + 90f, w, h), "Siguiente nivel"))
            LoadNextLevel();
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Puntos: {score}";
        if (collectiblesText != null) collectiblesText.text = $"Coleccionables: {collected}/{totalCollectibles}";
    }
}