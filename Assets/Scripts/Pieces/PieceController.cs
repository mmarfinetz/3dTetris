using System.Collections;
using UnityEngine;
using TetrisJenga.Core;
using TetrisJenga.Input;

namespace TetrisJenga.Pieces
{
    /// <summary>
    /// Controls the active piece during placement
    /// </summary>
    public class PieceController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float softDropSpeed = 2.0f;
        [SerializeField] private float hardDropSpeed = 20f;
        [SerializeField] private float normalFallSpeed = 0.4f;

        [Header("Placement Settings")]
        [SerializeField] private float placementDelay = 0.9f;
        [SerializeField] private float autoPlaceHeight = 0.2f;

        [Header("References")]
        [SerializeField] private GameObject currentPiece;
        [SerializeField] private GameObject ghostPiece;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private LayerMask placementLayer;

        // State
        private bool isControlling = false;
        private bool isDropping = false;
        private float currentFallSpeed;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Rigidbody currentRigidbody;
        private float placementTimer = 0f;

        // Components
        private InputManager inputManager;
        private PieceSpawner pieceSpawner;
        private GhostPiece ghostController;

        private void Awake()
        {
            inputManager = GetComponent<InputManager>();
            if (inputManager == null)
            {
                inputManager = gameObject.AddComponent<InputManager>();
            }

            pieceSpawner = GetComponent<PieceSpawner>();
            if (pieceSpawner == null)
            {
                pieceSpawner = gameObject.AddComponent<PieceSpawner>();
            }

            InitializeSpawnPoint();
        }

        private void InitializeSpawnPoint()
        {
            if (spawnPoint == null)
            {
                GameObject spawn = new GameObject("SpawnPoint");
                spawn.transform.position = new Vector3(0, Constants.PIECE_SPAWN_HEIGHT + 5f, 0);
                spawnPoint = spawn.transform;
            }
        }

        private void Start()
        {
            currentFallSpeed = normalFallSpeed;
            placementLayer = 1 << Constants.LAYER_PIECE | 1 << LayerMask.NameToLayer("Default");
        }

        private void Update()
        {
            if (!isControlling || currentPiece == null) return;

            HandleMovementInput();
            HandleRotationInput();
            HandleDropInput();
            UpdateGhostPiece();
            CheckAutoPlacement();
        }

        private void FixedUpdate()
        {
            if (!isControlling || currentPiece == null || currentRigidbody == null)
            {
                return;
            }

            // Apply movement
            if (!isDropping)
            {
                // Maintain horizontal position while allowing vertical fall
                Vector3 newPosition = currentRigidbody.position;
                newPosition.x = Mathf.Lerp(newPosition.x, targetPosition.x, Time.fixedDeltaTime * moveSpeed);
                newPosition.z = Mathf.Lerp(newPosition.z, targetPosition.z, Time.fixedDeltaTime * moveSpeed);
                currentRigidbody.MovePosition(newPosition);

                Quaternion newRotation = Quaternion.Lerp(
                    currentRigidbody.rotation,
                    targetRotation,
                    Time.fixedDeltaTime * rotationSpeed / 90f
                );
                currentRigidbody.MoveRotation(newRotation);

                // Boost downward velocity for faster falling
                if (currentFallSpeed > normalFallSpeed)
                {
                    Vector3 velocity = currentRigidbody.linearVelocity;
                    velocity.y = Mathf.Min(velocity.y, -currentFallSpeed);
                    currentRigidbody.linearVelocity = velocity;
                }
                else
                {
                    // Damp vertical velocity to ease falling
                    Vector3 velocity = currentRigidbody.linearVelocity;
                    if (Mathf.Abs(velocity.y) > currentFallSpeed)
                    {
                        velocity.y = Mathf.Lerp(velocity.y, -currentFallSpeed, 0.2f);
                        currentRigidbody.linearVelocity = velocity;
                    }
                }
            }
            else
            {
                // During hard drop, just boost downward velocity
                Vector3 velocity = currentRigidbody.linearVelocity;
                velocity.y = -hardDropSpeed;
                currentRigidbody.linearVelocity = velocity;
            }
        }

        /// <summary>
        /// Spawns a new piece and takes control
        /// </summary>
        public void SpawnNewPiece(PieceDefinition definition)
        {
            if (definition == null)
            {
                UnityEngine.Debug.LogError("Cannot spawn piece: definition is null!");
                return;
            }

            UnityEngine.Debug.Log($"SpawnNewPiece called with definition: {definition.name}");

            // Release current piece if any
            if (currentPiece != null)
            {
                ReleasePiece();
            }

            // Create new piece
            currentPiece = definition.CreatePiece();
            UnityEngine.Debug.Log($"Created piece: {currentPiece?.name ?? "null"}");

            if (spawnPoint == null)
            {
                UnityEngine.Debug.LogError("SpawnPoint is null! Initializing...");
                InitializeSpawnPoint();
            }

            if (spawnPoint != null)
            {
                currentPiece.transform.position = spawnPoint.position;
                currentPiece.transform.rotation = Quaternion.identity;
                UnityEngine.Debug.Log($"Piece positioned at: {currentPiece.transform.position}");
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to initialize spawn point!");
                currentPiece.transform.position = new Vector3(0, Constants.PIECE_SPAWN_HEIGHT, 0);
                UnityEngine.Debug.Log($"Using default position: {currentPiece.transform.position}");
            }

            // Get components
            currentRigidbody = currentPiece.GetComponent<Rigidbody>();
            if (currentRigidbody != null)
            {
                currentRigidbody.useGravity = true; // Let Unity handle gravity for simplicity
                currentRigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                // Keep the drag values from PieceDefinition, only override if they're too low
                if (currentRigidbody.linearDamping < 0.1f)
                {
                    currentRigidbody.linearDamping = 0.1f; // Minimum drag for control
                }
                UnityEngine.Debug.Log($"Rigidbody configured. UseGravity: {currentRigidbody.useGravity}, Constraints: {currentRigidbody.constraints}, Drag: {currentRigidbody.linearDamping}");
            }
            else
            {
                UnityEngine.Debug.LogError("No Rigidbody found on spawned piece!");
            }

            // Initialize control
            targetPosition = currentPiece.transform.position;
            targetRotation = currentPiece.transform.rotation;
            isControlling = true;
            isDropping = false;
            currentFallSpeed = normalFallSpeed;
            placementTimer = 0f;

            // Add collision handler to the piece
            PieceCollisionHandler collisionHandler = currentPiece.AddComponent<PieceCollisionHandler>();
            collisionHandler.Initialize(this);

            // Create ghost piece
            CreateGhostPiece(definition);
        }

        private void CreateGhostPiece(PieceDefinition definition)
        {
            if (ghostPiece != null)
            {
                Destroy(ghostPiece);
            }

            ghostPiece = definition.CreatePiece();
            ghostPiece.name = "GhostPiece";
            ghostPiece.layer = Constants.LAYER_GHOST;

            // Remove physics
            Rigidbody rb = ghostPiece.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Destroy(rb);
            }

            // Make translucent
            foreach (Renderer renderer in ghostPiece.GetComponentsInChildren<Renderer>())
            {
                Material mat = renderer.material;
                Color color = mat.color;
                color.a = Constants.GHOST_PREVIEW_ALPHA;
                mat.color = color;

                // Enable transparency
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }

            // Disable colliders
            foreach (Collider collider in ghostPiece.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            ghostController = ghostPiece.AddComponent<GhostPiece>();
        }

        private void HandleMovementInput()
        {
            if (isDropping) return;

            Vector3 movement = Vector3.zero;

            // Get camera-relative directions
            Camera cam = Camera.main;
            Vector3 forward = cam.transform.forward;
            Vector3 right = cam.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // WASD/Arrow movement
            if (UnityEngine.Input.GetKey(KeyCode.W) || UnityEngine.Input.GetKey(KeyCode.UpArrow))
            {
                movement += forward;
            }
            if (UnityEngine.Input.GetKey(KeyCode.S) || UnityEngine.Input.GetKey(KeyCode.DownArrow))
            {
                movement -= forward;
            }
            if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.LeftArrow))
            {
                movement -= right;
            }
            if (UnityEngine.Input.GetKey(KeyCode.D) || UnityEngine.Input.GetKey(KeyCode.RightArrow))
            {
                movement += right;
            }

            if (movement != Vector3.zero)
            {
                targetPosition += movement.normalized * moveSpeed * Time.deltaTime;

                // Clamp to boundaries
                targetPosition.x = Mathf.Clamp(targetPosition.x, -6f, 6f);
                targetPosition.z = Mathf.Clamp(targetPosition.z, -6f, 6f);
            }
        }

        private void HandleRotationInput()
        {
            if (isDropping) return;

            float rotationDelta = rotationSpeed * Time.deltaTime;

            // Q/E for Y-axis rotation
            if (UnityEngine.Input.GetKey(KeyCode.Q))
            {
                targetRotation *= Quaternion.Euler(0, -rotationDelta, 0);
            }
            if (UnityEngine.Input.GetKey(KeyCode.E))
            {
                targetRotation *= Quaternion.Euler(0, rotationDelta, 0);
            }

            // R/F for X-axis rotation
            if (UnityEngine.Input.GetKey(KeyCode.R))
            {
                targetRotation *= Quaternion.Euler(rotationDelta, 0, 0);
            }
            if (UnityEngine.Input.GetKey(KeyCode.F))
            {
                targetRotation *= Quaternion.Euler(-rotationDelta, 0, 0);
            }

            // Z/C for Z-axis rotation
            if (UnityEngine.Input.GetKey(KeyCode.Z))
            {
                targetRotation *= Quaternion.Euler(0, 0, rotationDelta);
            }
            if (UnityEngine.Input.GetKey(KeyCode.C))
            {
                targetRotation *= Quaternion.Euler(0, 0, -rotationDelta);
            }
        }

        private void HandleDropInput()
        {
            // Hard drop (Space)
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                StartHardDrop();
            }
            // Soft drop (Shift)
            else if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
            {
                currentFallSpeed = softDropSpeed;
            }
            else if (!isDropping)
            {
                currentFallSpeed = normalFallSpeed;
            }
        }

        private void StartHardDrop()
        {
            if (isDropping) return;

            isDropping = true;
            currentFallSpeed = hardDropSpeed;

            // Disable rotation constraints for more realistic physics
            if (currentRigidbody != null)
            {
                currentRigidbody.constraints = RigidbodyConstraints.None;
            }

            // Start fade animation on ghost
            if (ghostController != null)
            {
                ghostController.StartFadeOut();
            }
        }

        private void UpdateGhostPiece()
        {
            if (ghostPiece == null || currentPiece == null) return;

            // Cast ray down to find landing position
            Vector3 rayStart = currentPiece.transform.position;
            RaycastHit hit;

            if (UnityEngine.Physics.Raycast(rayStart, Vector3.down, out hit, 50f, placementLayer))
            {
                // Position ghost at landing spot
                float pieceHeight = GetPieceHeight(currentPiece);
                Vector3 ghostPosition = hit.point + Vector3.up * (pieceHeight * 0.5f);
                ghostPiece.transform.position = ghostPosition;
                ghostPiece.transform.rotation = currentPiece.transform.rotation;
            }
        }

        private float GetPieceHeight(GameObject piece)
        {
            Bounds bounds = new Bounds(piece.transform.position, Vector3.zero);
            foreach (Renderer renderer in piece.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds.size.y;
        }

        private void CheckAutoPlacement()
        {
            if (!isControlling || currentPiece == null || isDropping) return;

            // Check if piece has low velocity (essentially stopped moving)
            if (currentRigidbody != null)
            {
                float velocity = currentRigidbody.linearVelocity.magnitude;

                // Check if piece is close to ground or another piece
                RaycastHit hit;
                float checkDistance = autoPlaceHeight + GetPieceHeight(currentPiece);

                if (UnityEngine.Physics.Raycast(currentPiece.transform.position, Vector3.down, out hit, checkDistance, placementLayer))
                {
                    // If we're close to ground/pieces and moving slowly, start placement timer
                    if (velocity < 0.25f)
                    {
                        placementTimer += Time.deltaTime;

                        if (placementTimer >= placementDelay)
                        {
                            // Apply small upward adjustment to prevent tiny penetrations
                            currentRigidbody.position += Vector3.up * 0.01f;
                            PlacePiece();
                        }
                    }
                    else
                    {
                        placementTimer = 0f;
                    }
                }
                else
                {
                    placementTimer = 0f;
                }
            }
        }

        /// <summary>
        /// Places the current piece and releases control
        /// </summary>
        private void PlacePiece()
        {
            if (currentPiece == null) return;

            // Calculate placement score
            float placementScore = CalculatePlacementScore();

            // Release control
            ReleasePiece();

            // Notify game manager
            GameManager.Instance.OnPiecePlaced(currentPiece, placementScore);

            // Request next piece
            if (pieceSpawner != null)
            {
                pieceSpawner.RequestNextPiece();
            }
        }

        private float CalculatePlacementScore()
        {
            float score = 100f;

            // Deduct for rotation (pieces should be aligned)
            float rotationPenalty = Quaternion.Angle(currentPiece.transform.rotation, Quaternion.identity);
            score -= rotationPenalty * 0.5f;

            // Bonus for center placement
            float distanceFromCenter = Vector3.Distance(
                new Vector3(currentPiece.transform.position.x, 0, currentPiece.transform.position.z),
                Vector3.zero
            );
            score -= distanceFromCenter * 2f;

            return Mathf.Clamp(score, 0f, 100f);
        }

        /// <summary>
        /// Releases control of the current piece
        /// </summary>
        private void ReleasePiece()
        {
            if (currentPiece == null) return;

            UnityEngine.Debug.Log($"Releasing piece: {currentPiece.name}");
            isControlling = false;

            // Enable full physics
            if (currentRigidbody != null)
            {
                currentRigidbody.useGravity = true;
                currentRigidbody.constraints = RigidbodyConstraints.None;
                currentRigidbody.linearDamping = 0.1f; // Reset drag for natural physics
            }

            // Remove collision handler since we're done controlling
            PieceCollisionHandler handler = currentPiece.GetComponent<PieceCollisionHandler>();
            if (handler != null)
            {
                Destroy(handler);
            }

            // Clear ghost
            if (ghostPiece != null)
            {
                Destroy(ghostPiece);
                ghostPiece = null;
            }

            currentPiece = null;
            currentRigidbody = null;
        }

        /// <summary>
        /// Called by PieceCollisionHandler when a piece lands
        /// </summary>
        public void OnPieceLanded(GameObject piece)
        {
            if (piece != currentPiece) return;

            UnityEngine.Debug.Log($"Piece landed: {piece.name}");
            PlacePiece();
        }

        // Public methods for external control
        public bool IsControlling() => isControlling;
        public GameObject GetCurrentPiece() => currentPiece;
        public void ForceRelease() => ReleasePiece();
    }
}