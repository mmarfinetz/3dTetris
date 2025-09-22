using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TetrisJenga.Pieces;

namespace TetrisJenga.Core
{
    /// <summary>
    /// Central game manager responsible for game flow, state management, and system coordination
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Game Configuration")]
        [SerializeField] private float maxTowerHeight = 20f;
        [SerializeField] private float targetTowerHeight = 18f;
        [SerializeField] private float maxTiltAngle = 15f;
        [SerializeField] private float tiltFailureTime = 2f;
        [SerializeField] private float settlementTime = 0.5f;

        [Header("Scoring Configuration")]
        [SerializeField] private int pointsPerLayer = 10;
        [SerializeField] private float stabilityMultiplier = 2f;
        [SerializeField] private float timeBonus = 100f;
        [SerializeField] private float quickPlacementTime = 3f;
        [SerializeField] private int milestoneHeightStep = 3;
        [SerializeField] private int milestoneBonus = 150;

        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private int currentScore = 0;
        [SerializeField] private int highScore = 0;
        [SerializeField] private float currentHeight = 0f;
        [SerializeField] private float currentStability = 100f;
        [SerializeField] private int perfectStreakCount = 0;

        [Header("References")]
        [SerializeField] private Transform towerBase;
        [SerializeField] private Transform boundaryTop;
        [SerializeField] private Camera mainCamera;

        // Events
        public UnityEvent<GameState> OnStateChanged = new UnityEvent<GameState>();
        public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
        public UnityEvent<float> OnStabilityChanged = new UnityEvent<float>();
        public UnityEvent<float> OnHeightChanged = new UnityEvent<float>();
        public UnityEvent OnGameOver = new UnityEvent();
        public UnityEvent OnPerfectPlacement = new UnityEvent();

        // Private variables
        private float tiltTimer = 0f;
        private float placementTimer = 0f;
        private bool isTowerSettled = true;
        private int lastMilestoneAwarded = 0;
        private List<GameObject> activePieces = new List<GameObject>();
        private Coroutine gameLoopCoroutine;
        private PieceSpawner pieceSpawner;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            LoadHighScore();
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (towerBase == null)
            {
                GameObject baseObj = GameObject.Find("TowerBase");
                if (baseObj == null)
                {
                    baseObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    baseObj.name = "TowerBase";
                    baseObj.transform.localScale = new Vector3(12f, 0.5f, 12f);
                    baseObj.transform.position = Vector3.zero;
                }
                towerBase = baseObj.transform;
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (boundaryTop == null)
            {
                GameObject boundary = new GameObject("BoundaryTop");
                boundary.transform.position = new Vector3(0, maxTowerHeight, 0);
                boundaryTop = boundary.transform;
            }

            if (pieceSpawner == null)
            {
                pieceSpawner = FindFirstObjectByType<PieceSpawner>();
            }
        }

        private void Start()
        {
            // Clean up any stray pieces that might exist in the scene
            CleanupStrayPieces();
            ChangeState(GameState.MainMenu);
        }

        private void CleanupStrayPieces()
        {
            // Find all objects with piece tag that aren't in our active pieces list
            GameObject[] allPieces = GameObject.FindGameObjectsWithTag(Constants.TAG_PIECE);
            foreach (GameObject piece in allPieces)
            {
                if (!activePieces.Contains(piece))
                {
                    UnityEngine.Debug.Log($"Cleaning up stray piece: {piece.name} at position {piece.transform.position}");
                    Destroy(piece);
                }
            }

            // Also clean up any preview pieces
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("Preview_"))
                {
                    UnityEngine.Debug.Log($"Cleaning up stray preview: {obj.name}");
                    Destroy(obj);
                }
            }
        }

        /// <summary>
        /// Changes the current game state and triggers appropriate actions
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            // Exit current state
            OnStateExit(currentState);

            // Change state
            GameState previousState = currentState;
            currentState = newState;

            // Enter new state
            OnStateEnter(newState);

            // Notify listeners
            OnStateChanged?.Invoke(newState);

            UnityEngine.Debug.Log($"Game State Changed: {previousState} -> {newState}");
        }

        private void OnStateEnter(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    if (gameLoopCoroutine != null)
                    {
                        StopCoroutine(gameLoopCoroutine);
                        gameLoopCoroutine = null;
                    }
                    break;

                case GameState.Playing:
                    ResetGame();
                    Time.timeScale = 1f;
                    gameLoopCoroutine = StartCoroutine(GameLoop());
                    // Spawn the first controllable piece with a small delay
                    if (pieceSpawner == null)
                    {
                        pieceSpawner = FindFirstObjectByType<PieceSpawner>();
                        UnityEngine.Debug.Log($"Found PieceSpawner: {pieceSpawner != null}");
                    }
                    if (pieceSpawner != null)
                    {
                        StartCoroutine(SpawnFirstPiece());
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("PieceSpawner not found! Cannot spawn pieces.");
                    }
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0.3f; // Slow motion for collapse
                    if (currentScore > highScore)
                    {
                        highScore = currentScore;
                        SaveHighScore();
                    }
                    OnGameOver?.Invoke();
                    StartCoroutine(GameOverSequence());
                    break;

                case GameState.Victory:
                    Time.timeScale = 1f;
                    if (currentScore > highScore)
                    {
                        highScore = currentScore;
                        SaveHighScore();
                    }
                    UnityEngine.Debug.Log($"Victory achieved! Final height: {currentHeight}m, Score: {currentScore}");
                    OnGameOver?.Invoke(); // Reuse game over event for now
                    break;
            }
        }

        private void OnStateExit(GameState state)
        {
            switch (state)
            {
                case GameState.Paused:
                    Time.timeScale = 1f;
                    break;
            }
        }

        /// <summary>
        /// Main game loop that runs during gameplay
        /// </summary>
        private IEnumerator GameLoop()
        {
            while (currentState == GameState.Playing)
            {
                // Update placement timer
                placementTimer += Time.deltaTime;

                // Check win/loss conditions
                CheckTowerStability();
                CheckTowerHeight();
                CheckTowerTilt();

                // Update UI
                OnStabilityChanged?.Invoke(currentStability);
                OnHeightChanged?.Invoke(currentHeight);

                yield return null;
            }
        }

        private void CheckTowerStability()
        {
            // This will be updated by StabilityAnalyzer
            if (currentStability <= 0 && isTowerSettled)
            {
                TriggerGameOver("Tower collapsed!");
            }
        }

        private void CheckTowerHeight()
        {
            float maxY = 0f;
            foreach (GameObject piece in activePieces)
            {
                if (piece != null)
                {
                    float pieceTop = piece.transform.position.y +
                                   piece.GetComponent<Collider>().bounds.extents.y;
                    maxY = Mathf.Max(maxY, pieceTop);
                }
            }

            currentHeight = maxY;

            // Check for victory condition
            if (currentHeight >= targetTowerHeight && isTowerSettled)
            {
                UnityEngine.Debug.Log($"Victory! Tower reached target height of {targetTowerHeight}m");
                ChangeState(GameState.Victory);
            }
            else if (currentHeight > maxTowerHeight)
            {
                TriggerGameOver("Tower too tall!");
            }
        }

        private void CheckTowerTilt()
        {
            if (activePieces.Count == 0) return;

            Vector3 centerOfMass = CalculateTowerCenterOfMass();
            Vector3 baseCenter = towerBase.position;

            Vector3 tiltVector = centerOfMass - baseCenter;
            tiltVector.y = 0;

            float tiltAngle = Mathf.Atan2(tiltVector.magnitude, Mathf.Abs(centerOfMass.y - baseCenter.y)) * Mathf.Rad2Deg;

            if (tiltAngle > maxTiltAngle)
            {
                tiltTimer += Time.deltaTime;
                if (tiltTimer >= tiltFailureTime)
                {
                    TriggerGameOver("Tower tilted too far!");
                }
            }
            else
            {
                tiltTimer = 0f;
            }
        }

        private Vector3 CalculateTowerCenterOfMass()
        {
            Vector3 weightedSum = Vector3.zero;
            float totalMass = 0f;

            foreach (GameObject piece in activePieces)
            {
                if (piece != null)
                {
                    Rigidbody rb = piece.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        weightedSum += rb.worldCenterOfMass * rb.mass;
                        totalMass += rb.mass;
                    }
                }
            }

            return totalMass > 0 ? weightedSum / totalMass : Vector3.zero;
        }

        /// <summary>
        /// Called when a piece is successfully placed
        /// </summary>
        public void OnPiecePlaced(GameObject piece, float placementScore)
        {
            if (currentState != GameState.Playing) return;

            activePieces.Add(piece);

            // Calculate score
            int layerBonus = Mathf.FloorToInt(currentHeight) * pointsPerLayer;
            int stabilityBonus = Mathf.RoundToInt(currentStability * stabilityMultiplier);
            int timeBonus = placementTimer < quickPlacementTime ? Mathf.RoundToInt(this.timeBonus) : 0;

            int totalScore = layerBonus + stabilityBonus + timeBonus;

            // Check for perfect placement
            if (placementScore >= 90f)
            {
                perfectStreakCount++;
                totalScore = Mathf.RoundToInt(totalScore * (1f + perfectStreakCount * 0.1f));
                OnPerfectPlacement?.Invoke();
            }
            else
            {
                perfectStreakCount = 0;
            }

            AddScore(totalScore);

            // Check for height milestones
            int currentMilestone = Mathf.FloorToInt(currentHeight / milestoneHeightStep);
            if (currentMilestone > lastMilestoneAwarded)
            {
                int milestonesEarned = currentMilestone - lastMilestoneAwarded;
                int milestonePoints = milestonesEarned * milestoneBonus;
                AddScore(milestonePoints);
                lastMilestoneAwarded = currentMilestone;
                UnityEngine.Debug.Log($"Height milestone reached! Level {currentMilestone} - Bonus: {milestonePoints}");
            }

            // Reset placement timer
            placementTimer = 0f;

            // Start settlement detection
            isTowerSettled = false;
            StartCoroutine(WaitForSettlement());
        }

        private IEnumerator WaitForSettlement()
        {
            yield return new WaitForSeconds(settlementTime);

            // Check if all pieces have settled
            bool allSettled = true;
            foreach (GameObject piece in activePieces)
            {
                if (piece != null)
                {
                    Rigidbody rb = piece.GetComponent<Rigidbody>();
                    if (rb != null && rb.linearVelocity.magnitude > 0.01f)
                    {
                        allSettled = false;
                        break;
                    }
                }
            }

            if (allSettled)
            {
                isTowerSettled = true;
            }
            else
            {
                StartCoroutine(WaitForSettlement());
            }
        }

        /// <summary>
        /// Adds score and updates UI
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Updates the tower stability value
        /// </summary>
        public void UpdateStability(float stability)
        {
            currentStability = Mathf.Clamp(stability, 0f, 100f);
            OnStabilityChanged?.Invoke(currentStability);
        }

        /// <summary>
        /// Triggers game over with a reason
        /// </summary>
        public void TriggerGameOver(string reason)
        {
            if (currentState == GameState.GameOver) return;

            UnityEngine.Debug.Log($"Game Over: {reason}");
            ChangeState(GameState.GameOver);
        }

        private IEnumerator GameOverSequence()
        {
            // Wait for dramatic effect
            yield return new WaitForSecondsRealtime(3f);

            // Show game over UI
            Time.timeScale = 1f;
        }

        private IEnumerator SpawnFirstPiece()
        {
            // Wait a frame to ensure everything is initialized
            yield return null;

            UnityEngine.Debug.Log("Spawning first piece after delay...");
            if (pieceSpawner != null)
            {
                pieceSpawner.RequestNextPiece();
            }
        }

        /// <summary>
        /// Resets the game to initial state
        /// </summary>
        public void ResetGame()
        {
            // Clear pieces
            foreach (GameObject piece in activePieces)
            {
                if (piece != null)
                {
                    Destroy(piece);
                }
            }
            activePieces.Clear();

            // Reset variables
            currentScore = 0;
            currentHeight = 0f;
            currentStability = 100f;
            perfectStreakCount = 0;
            tiltTimer = 0f;
            placementTimer = 0f;
            isTowerSettled = true;
            lastMilestoneAwarded = 0;

            // Notify UI
            OnScoreChanged?.Invoke(currentScore);
            OnStabilityChanged?.Invoke(currentStability);
            OnHeightChanged?.Invoke(currentHeight);
        }

        /// <summary>
        /// Starts a new game
        /// </summary>
        public void StartGame()
        {
            ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Pauses the game
        /// </summary>
        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        /// <summary>
        /// Resumes the game
        /// </summary>
        public void ResumeGame()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        /// <summary>
        /// Returns to main menu
        /// </summary>
        public void ReturnToMenu()
        {
            ChangeState(GameState.MainMenu);
        }

        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // Getters
        public GameState GetCurrentState() => currentState;
        public int GetCurrentScore() => currentScore;
        public int GetHighScore() => highScore;
        public float GetCurrentHeight() => currentHeight;
        public float GetCurrentStability() => currentStability;
        public List<GameObject> GetActivePieces() => new List<GameObject>(activePieces);
        public bool IsTowerSettled() => isTowerSettled;
    }
}