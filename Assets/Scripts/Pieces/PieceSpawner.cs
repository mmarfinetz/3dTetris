using System.Collections.Generic;
using UnityEngine;
using TetrisJenga.Core;

namespace TetrisJenga.Pieces
{
    /// <summary>
    /// Manages piece spawning and next piece queue
    /// </summary>
    public class PieceSpawner : MonoBehaviour
    {
        [Header("Piece Configuration")]
        [SerializeField] private List<PieceDefinition> availablePieces = new List<PieceDefinition>();
        [SerializeField] private int queueSize = 3;
        [SerializeField] private bool useBagRandomization = true;

        [Header("Preview Configuration")]
        [SerializeField] private bool enablePreviews = false; // Disabled by default to prevent stray pieces
        [SerializeField] private Transform[] previewPositions;
        [SerializeField] private float previewScale = 0.5f;

        // Queue management
        private Queue<PieceDefinition> nextPiecesQueue = new Queue<PieceDefinition>();
        private List<GameObject> previewObjects = new List<GameObject>();
        private List<PieceDefinition> currentBag = new List<PieceDefinition>();

        // Components
        private PieceController pieceController;

        private void Awake()
        {
            pieceController = GetComponent<PieceController>();
            if (pieceController == null)
            {
                pieceController = gameObject.AddComponent<PieceController>();
            }

            InitializePieces();
            InitializePreviewPositions();
        }

        private void InitializePieces()
        {
            if (availablePieces.Count == 0)
            {
                // Create default pieces if none are assigned
                availablePieces = PieceDefinition.CreateDefaultPieces();

                // In a real project, these would be saved as ScriptableObject assets
                UnityEngine.Debug.Log($"Created {availablePieces.Count} default piece definitions");
            }

            // Validate all pieces
            for (int i = availablePieces.Count - 1; i >= 0; i--)
            {
                if (!availablePieces[i].Validate())
                {
                    UnityEngine.Debug.LogError($"Removing invalid piece definition at index {i}");
                    availablePieces.RemoveAt(i);
                }
            }
        }

        private void InitializePreviewPositions()
        {
            if (previewPositions == null || previewPositions.Length == 0)
            {
                previewPositions = new Transform[queueSize];

                GameObject previewContainer = new GameObject("PreviewContainer");
                previewContainer.transform.position = new Vector3(7, 10, 0);

                for (int i = 0; i < queueSize; i++)
                {
                    GameObject previewSlot = new GameObject($"PreviewSlot_{i}");
                    previewSlot.transform.SetParent(previewContainer.transform);
                    previewSlot.transform.localPosition = new Vector3(0, -i * 3, 0);
                    previewPositions[i] = previewSlot.transform;
                }
            }
        }

        private void Start()
        {
            FillQueue();

            // Clean up any existing preview pieces from the scene
            CleanupStrayPreviews();

            // Only create preview display if enabled and properly configured
            if (enablePreviews && previewPositions != null && previewPositions.Length > 0)
            {
                bool validPositions = true;
                foreach (var pos in previewPositions)
                {
                    if (pos == null)
                    {
                        validPositions = false;
                        break;
                    }
                }

                if (validPositions)
                {
                    UpdatePreviewDisplay();
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Preview positions contain null entries - disabling preview display");
                }
            }
            else if (enablePreviews)
            {
                UnityEngine.Debug.Log("Previews enabled but positions not configured");
            }
        }

        private void CleanupStrayPreviews()
        {
            // Find and destroy any preview pieces that might be in the scene
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.StartsWith("Preview_") && !previewObjects.Contains(obj))
                {
                    UnityEngine.Debug.Log($"Cleaning up stray preview object: {obj.name}");
                    Destroy(obj);
                }
            }
        }

        /// <summary>
        /// Fills the next piece queue
        /// </summary>
        private void FillQueue()
        {
            while (nextPiecesQueue.Count < queueSize)
            {
                PieceDefinition nextPiece = GetRandomPiece();
                nextPiecesQueue.Enqueue(nextPiece);
            }
        }

        /// <summary>
        /// Gets a random piece using bag randomization or pure random
        /// </summary>
        private PieceDefinition GetRandomPiece()
        {
            if (availablePieces.Count == 0)
            {
                UnityEngine.Debug.LogError("No pieces available!");
                return null;
            }

            if (useBagRandomization)
            {
                // Bag randomization ensures all pieces appear before repeating
                if (currentBag.Count == 0)
                {
                    RefillBag();
                }

                int index = Random.Range(0, currentBag.Count);
                PieceDefinition piece = currentBag[index];
                currentBag.RemoveAt(index);
                return piece;
            }
            else
            {
                // Pure random
                return availablePieces[Random.Range(0, availablePieces.Count)];
            }
        }

        private void RefillBag()
        {
            currentBag.Clear();
            currentBag.AddRange(availablePieces);

            // Shuffle the bag
            for (int i = currentBag.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                PieceDefinition temp = currentBag[i];
                currentBag[i] = currentBag[randomIndex];
                currentBag[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Spawns the next piece and refills the queue
        /// </summary>
        public void RequestNextPiece()
        {
            UnityEngine.Debug.Log("RequestNextPiece called");

            if (GameManager.Instance.GetCurrentState() != GameState.Playing)
            {
                UnityEngine.Debug.Log($"Not in playing state: {GameManager.Instance.GetCurrentState()}");
                return;
            }

            if (pieceController == null)
            {
                UnityEngine.Debug.LogError("PieceController is null!");
                return;
            }

            if (pieceController.IsControlling())
            {
                UnityEngine.Debug.LogWarning("Cannot spawn new piece - controller is busy!");
                return;
            }

            if (nextPiecesQueue.Count == 0)
            {
                UnityEngine.Debug.Log("Filling queue...");
                FillQueue();
            }

            PieceDefinition nextPiece = nextPiecesQueue.Dequeue();
            UnityEngine.Debug.Log($"Spawning piece: {nextPiece?.name ?? "null"}");
            FillQueue();

            // Only update preview display if previews are enabled
            if (enablePreviews)
            {
                UpdatePreviewDisplay();
            }

            pieceController.SpawnNewPiece(nextPiece);
        }

        /// <summary>
        /// Updates the preview display
        /// </summary>
        private void UpdatePreviewDisplay()
        {
            // Clear old previews
            foreach (GameObject preview in previewObjects)
            {
                if (preview != null)
                {
                    Destroy(preview);
                }
            }
            previewObjects.Clear();

            // Skip creating previews if positions aren't properly set up
            if (previewPositions == null || previewPositions.Length == 0)
            {
                UnityEngine.Debug.Log("Preview positions not configured - skipping preview display");
                return;
            }

            // Create new previews
            int index = 0;
            foreach (PieceDefinition piece in nextPiecesQueue)
            {
                if (index >= previewPositions.Length) break;

                // Skip if this preview position is null
                if (previewPositions[index] == null)
                {
                    UnityEngine.Debug.LogWarning($"Preview position {index} is null - skipping");
                    index++;
                    continue;
                }

                GameObject previewObj = CreatePreviewObject(piece);
                previewObj.transform.position = previewPositions[index].position;
                previewObj.transform.localScale = Vector3.one * previewScale;

                previewObjects.Add(previewObj);
                index++;
            }
        }

        private GameObject CreatePreviewObject(PieceDefinition definition)
        {
            GameObject preview = definition.CreatePiece();
            preview.name = $"Preview_{definition.PieceName}";
            preview.layer = Constants.LAYER_UI;

            // Remove physics
            Rigidbody rb = preview.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }

            // Disable colliders
            foreach (Collider col in preview.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Make slightly transparent
            foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>())
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = 0.8f;
                mat.color = color;
            }

            // Add slow rotation for visual interest
            PreviewRotator rotator = preview.AddComponent<PreviewRotator>();
            rotator.rotationSpeed = 30f;

            return preview;
        }

        /// <summary>
        /// Gets the current queue for UI display
        /// </summary>
        public List<PieceDefinition> GetNextPieces()
        {
            return new List<PieceDefinition>(nextPiecesQueue);
        }

        /// <summary>
        /// Resets the spawner
        /// </summary>
        public void Reset()
        {
            nextPiecesQueue.Clear();
            currentBag.Clear();

            foreach (GameObject preview in previewObjects)
            {
                if (preview != null)
                {
                    Destroy(preview);
                }
            }
            previewObjects.Clear();

            FillQueue();

            // Only update preview display if previews are enabled
            if (enablePreviews)
            {
                UpdatePreviewDisplay();
            }
        }
    }

    /// <summary>
    /// Simple component to rotate preview pieces
    /// </summary>
    public class PreviewRotator : MonoBehaviour
    {
        public float rotationSpeed = 30f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
}