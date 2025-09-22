using UnityEngine;
using TetrisJenga.Pieces;
using TetrisJenga.Physics;
using TetrisJenga.Input;

namespace TetrisJenga.Core
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Auto-Start Settings")]
        [SerializeField] private bool autoStartGame = true;
        [SerializeField] private float startDelay = 0.5f;

        private void Awake()
        {
            UnityEngine.Debug.Log("=== GAME INITIALIZER STARTING ===");

            GameManager gameManager = GameManager.Instance;
            UnityEngine.Debug.Log($"GameManager initialized: {gameManager != null}");

            GameObject gameController = GameObject.Find("GameController");
            if (gameController == null)
            {
                UnityEngine.Debug.Log("Creating GameController object...");
                gameController = new GameObject("GameController");
            }

            PieceController pieceController = gameController.GetComponent<PieceController>();
            if (pieceController == null)
            {
                UnityEngine.Debug.Log("Adding PieceController component...");
                pieceController = gameController.AddComponent<PieceController>();
            }

            PieceSpawner pieceSpawner = gameController.GetComponent<PieceSpawner>();
            if (pieceSpawner == null)
            {
                UnityEngine.Debug.Log("Adding PieceSpawner component...");
                pieceSpawner = gameController.AddComponent<PieceSpawner>();
            }

            InputManager inputManager = gameController.GetComponent<InputManager>();
            if (inputManager == null)
            {
                UnityEngine.Debug.Log("Adding InputManager component...");
                inputManager = gameController.AddComponent<InputManager>();
            }

            // Apply physics configuration
            // Using sensible defaults directly instead of creating a ScriptableObject
            UnityEngine.Debug.Log("Applying physics configuration...");
            UnityEngine.Physics.gravity = new Vector3(0, -9.81f, 0);
            Time.fixedDeltaTime = 0.02f;
            UnityEngine.Physics.defaultSolverIterations = 10;
            UnityEngine.Physics.defaultSolverVelocityIterations = 4;
            UnityEngine.Physics.defaultContactOffset = 0.01f;
            UnityEngine.Physics.sleepThreshold = 0.005f;
            UnityEngine.Physics.bounceThreshold = 1f;
            UnityEngine.Physics.defaultMaxAngularSpeed = 50f;
            UnityEngine.Physics.autoSyncTransforms = false;
            UnityEngine.Physics.reuseCollisionCallbacks = true;
            UnityEngine.Debug.Log($"Physics configured - Gravity: {UnityEngine.Physics.gravity}");

            GameObject basePlatform = GameObject.Find("TowerBase");
            if (basePlatform == null)
            {
                UnityEngine.Debug.Log("Creating base platform...");
                basePlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                basePlatform.name = "TowerBase";
                basePlatform.transform.position = Vector3.zero;
                basePlatform.transform.localScale = new Vector3(12f, 0.5f, 12f);
                basePlatform.tag = Constants.TAG_BASE;
                basePlatform.isStatic = true;

                Renderer renderer = basePlatform.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                UnityEngine.Debug.Log("Creating main camera...");
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            mainCamera.transform.position = new Vector3(10f, 10f, -10f);
            mainCamera.transform.LookAt(new Vector3(0, 5f, 0));

            CameraController cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                UnityEngine.Debug.Log("Adding CameraController component...");
                cameraController = mainCamera.gameObject.AddComponent<CameraController>();
            }

            UnityEngine.Debug.Log("=== GAME INITIALIZER COMPLETE ===");
        }

        private void Start()
        {
            if (autoStartGame)
            {
                Invoke(nameof(StartGameDelayed), startDelay);
            }
        }

        private void StartGameDelayed()
        {
            UnityEngine.Debug.Log("Auto-starting game...");
            GameState currentState = GameManager.Instance.GetCurrentState();

            if (currentState == GameState.MainMenu)
            {
                GameManager.Instance.StartGame();
                UnityEngine.Debug.Log("Game started!");
            }
            else
            {
                UnityEngine.Debug.Log($"Game already in state: {currentState}");
            }
        }
    }
}