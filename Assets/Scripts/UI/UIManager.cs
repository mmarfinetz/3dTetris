using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TetrisJenga.Core;
using TetrisJenga.Input;

namespace TetrisJenga.UI
{
    /// <summary>
    /// Manages all UI elements and screens
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject controlsOverlay;

        [Header("HUD Elements")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text heightText;
        [SerializeField] private StabilityGauge stabilityGauge;
        [SerializeField] private Transform nextPiecesContainer;

        [Header("Game Over Elements")]
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text gameOverReasonText;
        [SerializeField] private Text statsText;

        [Header("Animation")]
        [SerializeField] private float fadeTime = 0.3f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // References
        private GameManager gameManager;
        private InputManager inputManager;
        private CanvasGroup[] canvasGroups;

        private void Awake()
        {
            CreateUIIfNeeded();
            CacheCanvasGroups();
        }

        private void CreateUIIfNeeded()
        {
            // Create Canvas if not exists
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create panels if not assigned
            if (mainMenuPanel == null)
            {
                mainMenuPanel = CreatePanel("MainMenu", canvas.transform);
                CreateMainMenu(mainMenuPanel.transform);
            }

            if (hudPanel == null)
            {
                hudPanel = CreatePanel("HUD", canvas.transform);
                CreateHUD(hudPanel.transform);
            }

            if (pauseMenuPanel == null)
            {
                pauseMenuPanel = CreatePanel("PauseMenu", canvas.transform);
                CreatePauseMenu(pauseMenuPanel.transform);
            }

            if (gameOverPanel == null)
            {
                gameOverPanel = CreatePanel("GameOver", canvas.transform);
                CreateGameOverScreen(gameOverPanel.transform);
            }

            if (controlsOverlay == null)
            {
                controlsOverlay = CreatePanel("Controls", canvas.transform);
                CreateControlsOverlay(controlsOverlay.transform);
            }
        }

        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            CanvasGroup group = panel.AddComponent<CanvasGroup>();
            group.alpha = 0;
            panel.SetActive(false);

            return panel;
        }

        private void CreateMainMenu(Transform parent)
        {
            // Title
            GameObject titleObj = CreateText("Title", parent, "3D TETRIS JENGA", 48);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.anchoredPosition = Vector2.zero;

            // Play Button
            GameObject playBtn = CreateButton("PlayButton", parent, "PLAY", () =>
            {
                UnityEngine.Debug.Log("Play button clicked!");
                if (GameManager.Instance != null)
                {
                    UnityEngine.Debug.Log("Starting game...");
                    GameManager.Instance.StartGame();
                }
                else
                {
                    UnityEngine.Debug.LogError("GameManager.Instance is null!");
                }
            });
            RectTransform playRect = playBtn.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.5f);
            playRect.anchorMax = new Vector2(0.5f, 0.5f);
            playRect.sizeDelta = new Vector2(200, 50);
            playRect.anchoredPosition = Vector2.zero;

            // High Score
            GameObject highScore = CreateText("HighScore", parent, "High Score: 0", 24);
            highScoreText = highScore.GetComponent<Text>();
            RectTransform hsRect = highScore.GetComponent<RectTransform>();
            hsRect.anchorMin = new Vector2(0.5f, 0.3f);
            hsRect.anchorMax = new Vector2(0.5f, 0.3f);
            hsRect.anchoredPosition = Vector2.zero;

            // Quit Button
            GameObject quitBtn = CreateButton("QuitButton", parent, "QUIT", () =>
            {
                Application.Quit();
            });
            RectTransform quitRect = quitBtn.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0.5f, 0.2f);
            quitRect.anchorMax = new Vector2(0.5f, 0.2f);
            quitRect.sizeDelta = new Vector2(200, 50);
            quitRect.anchoredPosition = Vector2.zero;
        }

        private void CreateHUD(Transform parent)
        {
            // Score
            GameObject scoreObj = CreateText("Score", parent, "Score: 0", 32);
            scoreText = scoreObj.GetComponent<Text>();
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.pivot = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -20);

            // Height
            GameObject heightObj = CreateText("Height", parent, "Height: 0.0m", 24);
            heightText = heightObj.GetComponent<Text>();
            RectTransform heightRect = heightObj.GetComponent<RectTransform>();
            heightRect.anchorMin = new Vector2(0, 1);
            heightRect.anchorMax = new Vector2(0, 1);
            heightRect.pivot = new Vector2(0, 1);
            heightRect.anchoredPosition = new Vector2(20, -60);

            // Stability Gauge
            GameObject gaugeObj = new GameObject("StabilityGauge");
            gaugeObj.transform.SetParent(parent);
            RectTransform gaugeRect = gaugeObj.AddComponent<RectTransform>();
            stabilityGauge = gaugeObj.AddComponent<StabilityGauge>();
            gaugeRect.anchorMin = new Vector2(0.5f, 1);
            gaugeRect.anchorMax = new Vector2(0.5f, 1);
            gaugeRect.pivot = new Vector2(0.5f, 1);
            gaugeRect.sizeDelta = new Vector2(300, 40);
            gaugeRect.anchoredPosition = new Vector2(0, -20);

            // Next Pieces
            GameObject nextLabel = CreateText("NextLabel", parent, "NEXT", 20);
            RectTransform nextLabelRect = nextLabel.GetComponent<RectTransform>();
            nextLabelRect.anchorMin = new Vector2(1, 1);
            nextLabelRect.anchorMax = new Vector2(1, 1);
            nextLabelRect.pivot = new Vector2(1, 1);
            nextLabelRect.anchoredPosition = new Vector2(-20, -20);

            GameObject nextContainer = new GameObject("NextPieces");
            nextContainer.transform.SetParent(parent);
            RectTransform containerRect = nextContainer.AddComponent<RectTransform>();
            nextPiecesContainer = nextContainer.transform;
            containerRect.anchorMin = new Vector2(1, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(1, 1);
            containerRect.sizeDelta = new Vector2(100, 300);
            containerRect.anchoredPosition = new Vector2(-20, -50);
        }

        private void CreatePauseMenu(Transform parent)
        {
            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Title
            GameObject title = CreateText("Title", parent, "PAUSED", 48);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.anchoredPosition = Vector2.zero;

            // Resume Button
            GameObject resumeBtn = CreateButton("Resume", parent, "RESUME", () =>
            {
                GameManager.Instance?.ResumeGame();
            });
            RectTransform resumeRect = resumeBtn.GetComponent<RectTransform>();
            resumeRect.anchorMin = new Vector2(0.5f, 0.5f);
            resumeRect.anchorMax = new Vector2(0.5f, 0.5f);
            resumeRect.sizeDelta = new Vector2(200, 50);
            resumeRect.anchoredPosition = Vector2.zero;

            // Menu Button
            GameObject menuBtn = CreateButton("Menu", parent, "MAIN MENU", () =>
            {
                GameManager.Instance?.ReturnToMenu();
            });
            RectTransform menuRect = menuBtn.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.3f);
            menuRect.anchorMax = new Vector2(0.5f, 0.3f);
            menuRect.sizeDelta = new Vector2(200, 50);
            menuRect.anchoredPosition = Vector2.zero;
        }

        private void CreateGameOverScreen(Transform parent)
        {
            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.8f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Title
            GameObject title = CreateText("Title", parent, "GAME OVER", 48);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.anchoredPosition = Vector2.zero;

            // Reason
            GameObject reason = CreateText("Reason", parent, "", 24);
            gameOverReasonText = reason.GetComponent<Text>();
            RectTransform reasonRect = reason.GetComponent<RectTransform>();
            reasonRect.anchorMin = new Vector2(0.5f, 0.65f);
            reasonRect.anchorMax = new Vector2(0.5f, 0.65f);
            reasonRect.anchoredPosition = Vector2.zero;

            // Score
            GameObject score = CreateText("Score", parent, "Score: 0", 32);
            finalScoreText = score.GetComponent<Text>();
            RectTransform scoreRect = score.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
            scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
            scoreRect.anchoredPosition = Vector2.zero;

            // Stats
            GameObject stats = CreateText("Stats", parent, "", 20);
            statsText = stats.GetComponent<Text>();
            RectTransform statsRect = stats.GetComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0.35f);
            statsRect.anchorMax = new Vector2(0.5f, 0.35f);
            statsRect.anchoredPosition = Vector2.zero;

            // Retry Button
            GameObject retryBtn = CreateButton("Retry", parent, "RETRY", () =>
            {
                GameManager.Instance?.StartGame();
            });
            RectTransform retryRect = retryBtn.GetComponent<RectTransform>();
            retryRect.anchorMin = new Vector2(0.5f, 0.2f);
            retryRect.anchorMax = new Vector2(0.5f, 0.2f);
            retryRect.sizeDelta = new Vector2(200, 50);
            retryRect.anchoredPosition = new Vector2(-110, 0);

            // Menu Button
            GameObject menuBtn = CreateButton("Menu", parent, "MENU", () =>
            {
                GameManager.Instance?.ReturnToMenu();
            });
            RectTransform menuRect = menuBtn.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.2f);
            menuRect.anchorMax = new Vector2(0.5f, 0.2f);
            menuRect.sizeDelta = new Vector2(200, 50);
            menuRect.anchoredPosition = new Vector2(110, 0);
        }

        private void CreateControlsOverlay(Transform parent)
        {
            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(0.3f, 0.5f);
            bgRect.offsetMin = new Vector2(20, 20);
            bgRect.offsetMax = new Vector2(-20, -20);

            // Controls text
            string controlsText = "CONTROLS\n\n" +
                                "Movement: WASD/Arrows\n" +
                                "Rotate Y: Q/E\n" +
                                "Rotate X: R/F\n" +
                                "Rotate Z: Z/C\n" +
                                "Drop: Space\n" +
                                "Soft Drop: Shift\n" +
                                "Camera: Mouse Drag\n" +
                                "Zoom: Mouse Scroll\n" +
                                "Pause: ESC\n" +
                                "Toggle Help: H";

            GameObject controls = CreateText("Controls", parent, controlsText, 16);
            Text controlsTextComp = controls.GetComponent<Text>();
            controlsTextComp.alignment = TextAnchor.UpperLeft;
            RectTransform controlsRect = controls.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0, 0);
            controlsRect.anchorMax = new Vector2(0.3f, 0.5f);
            controlsRect.offsetMin = new Vector2(30, 30);
            controlsRect.offsetMax = new Vector2(-30, -30);
        }

        private GameObject CreateText(string name, Transform parent, string text, int fontSize)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            Text textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            textComp.fontSize = fontSize;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(400, 100);

            return textObj;
        }

        private GameObject CreateButton(string name, Transform parent, string text, System.Action onClick)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => onClick());

            GameObject textObj = CreateText(name + "_Text", buttonObj.transform, text, 20);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonObj;
        }

        private void CacheCanvasGroups()
        {
            canvasGroups = new CanvasGroup[]
            {
                mainMenuPanel?.GetComponent<CanvasGroup>(),
                hudPanel?.GetComponent<CanvasGroup>(),
                pauseMenuPanel?.GetComponent<CanvasGroup>(),
                gameOverPanel?.GetComponent<CanvasGroup>(),
                controlsOverlay?.GetComponent<CanvasGroup>()
            };
        }

        private void Start()
        {
            gameManager = GameManager.Instance;
            inputManager = FindFirstObjectByType<InputManager>();

            // Subscribe to events
            if (gameManager != null)
            {
                gameManager.OnStateChanged.AddListener(OnGameStateChanged);
                gameManager.OnScoreChanged.AddListener(OnScoreChanged);
                gameManager.OnStabilityChanged.AddListener(OnStabilityChanged);
                gameManager.OnHeightChanged.AddListener(OnHeightChanged);
                gameManager.OnGameOver.AddListener(OnGameOver);
            }

            if (inputManager != null)
            {
                inputManager.OnPauseInput.AddListener(OnPausePressed);
                inputManager.OnShowControlsInput.AddListener(ToggleControls);
            }

            // Show main menu
            ShowPanel(mainMenuPanel);
        }

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowPanel(mainMenuPanel);
                    HidePanel(hudPanel);
                    HidePanel(pauseMenuPanel);
                    HidePanel(gameOverPanel);
                    break;

                case GameState.Playing:
                    HidePanel(mainMenuPanel);
                    ShowPanel(hudPanel);
                    HidePanel(pauseMenuPanel);
                    HidePanel(gameOverPanel);
                    break;

                case GameState.Paused:
                    ShowPanel(pauseMenuPanel);
                    break;

                case GameState.GameOver:
                    HidePanel(hudPanel);
                    ShowPanel(gameOverPanel);
                    break;
            }
        }

        private void OnScoreChanged(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }

        private void OnStabilityChanged(float stability)
        {
            if (stabilityGauge != null)
            {
                stabilityGauge.SetStability(stability);
            }
        }

        private void OnHeightChanged(float height)
        {
            if (heightText != null)
            {
                heightText.text = $"Height: {height:F1}m";
            }
        }

        private void OnGameOver()
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {gameManager.GetCurrentScore()}";
            }

            if (statsText != null)
            {
                float height = gameManager.GetCurrentHeight();
                statsText.text = $"Tower Height: {height:F1}m";
            }
        }

        private void OnPausePressed()
        {
            if (gameManager?.GetCurrentState() == GameState.Playing)
            {
                gameManager.PauseGame();
            }
            else if (gameManager?.GetCurrentState() == GameState.Paused)
            {
                gameManager.ResumeGame();
            }
        }

        private void ToggleControls()
        {
            if (controlsOverlay != null)
            {
                bool isActive = controlsOverlay.activeSelf;
                if (isActive)
                {
                    HidePanel(controlsOverlay);
                }
                else
                {
                    ShowPanel(controlsOverlay);
                }
            }
        }

        private void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
                StartCoroutine(FadePanel(panel.GetComponent<CanvasGroup>(), 1f));
            }
        }

        private void HidePanel(GameObject panel)
        {
            if (panel != null)
            {
                StartCoroutine(FadePanel(panel.GetComponent<CanvasGroup>(), 0f, () =>
                {
                    panel.SetActive(false);
                }));
            }
        }

        private IEnumerator FadePanel(CanvasGroup group, float targetAlpha, System.Action onComplete = null)
        {
            if (group == null) yield break;

            float startAlpha = group.alpha;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / fadeTime;
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, fadeCurve.Evaluate(t));
                yield return null;
            }

            group.alpha = targetAlpha;
            group.interactable = targetAlpha > 0;
            group.blocksRaycasts = targetAlpha > 0;

            onComplete?.Invoke();
        }

        /// <summary>
        /// Updates the high score display
        /// </summary>
        public void UpdateHighScore(int score)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {score}";
            }
        }
    }
}