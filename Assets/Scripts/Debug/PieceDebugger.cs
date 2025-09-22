using UnityEngine;
using TetrisJenga.Pieces;
using TetrisJenga.Core;

namespace TetrisJenga.Debug
{
    /// <summary>
    /// Debug helper to diagnose piece spawning and physics issues
    /// </summary>
    public class PieceDebugger : MonoBehaviour
    {
        private PieceController pieceController;
        private PieceSpawner pieceSpawner;
        private float debugTimer = 0f;

        void Start()
        {
            pieceController = FindFirstObjectByType<PieceController>();
            pieceSpawner = FindFirstObjectByType<PieceSpawner>();

            UnityEngine.Debug.Log("=== PIECE DEBUGGER STARTED ===");
            UnityEngine.Debug.Log($"GameManager State: {GameManager.Instance.GetCurrentState()}");
            UnityEngine.Debug.Log($"Time.timeScale: {Time.timeScale}");
            UnityEngine.Debug.Log($"PieceController found: {pieceController != null}");
            UnityEngine.Debug.Log($"PieceSpawner found: {pieceSpawner != null}");

            // Force start the game if in menu
            if (GameManager.Instance.GetCurrentState() == GameState.MainMenu)
            {
                UnityEngine.Debug.Log("Game in menu state - Starting game!");
                GameManager.Instance.StartGame();
            }
        }

        void Update()
        {
            debugTimer += Time.deltaTime;

            // Log every 2 seconds
            if (debugTimer >= 2f)
            {
                debugTimer = 0f;

                UnityEngine.Debug.Log("=== PERIODIC DEBUG ===");
                UnityEngine.Debug.Log($"Game State: {GameManager.Instance.GetCurrentState()}");
                UnityEngine.Debug.Log($"Time.timeScale: {Time.timeScale}");

                if (pieceController != null)
                {
                    UnityEngine.Debug.Log($"PieceController.IsControlling: {pieceController.IsControlling()}");
                    GameObject currentPiece = pieceController.GetCurrentPiece();
                    UnityEngine.Debug.Log($"Current Piece: {(currentPiece != null ? currentPiece.name : "null")}");

                    if (currentPiece != null)
                    {
                        Rigidbody rb = currentPiece.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            UnityEngine.Debug.Log($"Rigidbody.useGravity: {rb.useGravity}");
                            UnityEngine.Debug.Log($"Rigidbody.velocity: {rb.linearVelocity}");
                            UnityEngine.Debug.Log($"Rigidbody.constraints: {rb.constraints}");
                            UnityEngine.Debug.Log($"Piece Position: {currentPiece.transform.position}");
                        }
                    }
                }

                // Check for any pieces in the scene
                GameObject[] allPieces = GameObject.FindGameObjectsWithTag(Constants.TAG_PIECE);
                UnityEngine.Debug.Log($"Total pieces in scene: {allPieces.Length}");
                foreach (var piece in allPieces)
                {
                    UnityEngine.Debug.Log($"  - {piece.name} at {piece.transform.position}");
                }
            }

            // Test manual spawn with key press
            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                UnityEngine.Debug.Log("=== MANUAL SPAWN TEST (P key pressed) ===");
                if (pieceSpawner != null)
                {
                    pieceSpawner.RequestNextPiece();
                }
                else
                {
                    UnityEngine.Debug.LogError("PieceSpawner is null!");
                }
            }
        }
    }
}