using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TetrisJenga.Core;

namespace TetrisJenga.Input
{
    /// <summary>
    /// Manages all input handling with buffering support
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private float inputBufferTime = 0.1f;
        [SerializeField] private float repeatDelay = 0.5f;
        [SerializeField] private float repeatRate = 0.1f;
        [SerializeField] private bool enableInputBuffering = true;

        [Header("Sensitivity")]
        [SerializeField] private float movementSensitivity = 1f;
        [SerializeField] private float rotationSensitivity = 1f;
        [SerializeField] private float cameraSensitivity = 1f;

        // Input actions
        public UnityEvent<Vector3> OnMovementInput = new UnityEvent<Vector3>();
        public UnityEvent<Vector3> OnRotationInput = new UnityEvent<Vector3>();
        public UnityEvent OnHardDropInput = new UnityEvent();
        public UnityEvent OnSoftDropInput = new UnityEvent();
        public UnityEvent OnPauseInput = new UnityEvent();
        public UnityEvent OnShowControlsInput = new UnityEvent();

        // Camera input
        public UnityEvent<float> OnCameraRotateInput = new UnityEvent<float>();
        public UnityEvent<float> OnCameraZoomInput = new UnityEvent<float>();

        // Input buffer
        private Queue<BufferedInput> inputBuffer = new Queue<BufferedInput>();
        private Dictionary<KeyCode, float> keyHoldTimers = new Dictionary<KeyCode, float>();
        private Dictionary<KeyCode, float> keyRepeatTimers = new Dictionary<KeyCode, float>();

        // State
        private bool inputEnabled = true;
        private Vector3 currentMovement;
        private Vector3 currentRotation;
        private bool isHardDropping = false;
        private bool isSoftDropping = false;

        // Mouse state for camera
        private Vector3 lastMousePosition;
        private bool isDragging = false;

        private void Start()
        {
            // Subscribe to game state changes
            GameManager.Instance?.OnStateChanged.AddListener(OnGameStateChanged);
        }

        private void Update()
        {
            if (!inputEnabled) return;

            // Process buffered inputs
            ProcessInputBuffer();

            // Gather inputs
            GatherMovementInput();
            GatherRotationInput();
            GatherDropInput();
            GatherSystemInput();
            GatherCameraInput();

            // Process key repeats
            ProcessKeyRepeats();
        }

        private void GatherMovementInput()
        {
            Vector3 movement = Vector3.zero;

            // Get camera-relative directions
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 forward = cam.transform.forward;
                Vector3 right = cam.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                // WASD/Arrow movement
                if (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow))
                {
                    movement += forward;
                }
                if (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow))
                {
                    movement -= forward;
                }
                if (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow))
                {
                    movement -= right;
                }
                if (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow))
                {
                    movement += right;
                }
            }

            if (movement != currentMovement)
            {
                currentMovement = movement.normalized * movementSensitivity;
                OnMovementInput?.Invoke(currentMovement);
            }
        }

        private void GatherRotationInput()
        {
            Vector3 rotation = Vector3.zero;

            // Y-axis rotation (Q/E)
            if (GetKey(KeyCode.Q))
            {
                rotation.y = -1;
            }
            else if (GetKey(KeyCode.E))
            {
                rotation.y = 1;
            }

            // X-axis rotation (R/F)
            if (GetKey(KeyCode.R))
            {
                rotation.x = 1;
            }
            else if (GetKey(KeyCode.F))
            {
                rotation.x = -1;
            }

            // Z-axis rotation (Z/C)
            if (GetKey(KeyCode.Z))
            {
                rotation.z = 1;
            }
            else if (GetKey(KeyCode.C))
            {
                rotation.z = -1;
            }

            if (rotation != currentRotation)
            {
                currentRotation = rotation * rotationSensitivity;
                OnRotationInput?.Invoke(currentRotation);
            }
        }

        private void GatherDropInput()
        {
            // Hard drop (Space)
            if (GetKeyDown(KeyCode.Space))
            {
                if (enableInputBuffering)
                {
                    BufferInput(new BufferedInput
                    {
                        type = InputType.HardDrop,
                        timestamp = Time.time
                    });
                }
                OnHardDropInput?.Invoke();
                isHardDropping = true;
            }

            // Soft drop (Shift)
            bool softDrop = GetKey(KeyCode.LeftShift) || GetKey(KeyCode.RightShift);
            if (softDrop != isSoftDropping)
            {
                isSoftDropping = softDrop;
                if (softDrop)
                {
                    OnSoftDropInput?.Invoke();
                }
            }
        }

        private void GatherSystemInput()
        {
            // Pause (ESC)
            if (GetKeyDown(KeyCode.Escape))
            {
                OnPauseInput?.Invoke();
            }

            // Show controls (H)
            if (GetKeyDown(KeyCode.H))
            {
                OnShowControlsInput?.Invoke();
            }
        }

        private void GatherCameraInput()
        {
            // Mouse drag for camera rotation
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = UnityEngine.Input.mousePosition;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 mouseDelta = UnityEngine.Input.mousePosition - lastMousePosition;
                float rotationAmount = mouseDelta.x * cameraSensitivity;

                if (Mathf.Abs(rotationAmount) > 0.01f)
                {
                    OnCameraRotateInput?.Invoke(rotationAmount);
                }

                lastMousePosition = UnityEngine.Input.mousePosition;
            }

            // Mouse scroll for zoom
            float scrollDelta = UnityEngine.Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                OnCameraZoomInput?.Invoke(scrollDelta * cameraSensitivity);
            }
        }

        private void BufferInput(BufferedInput input)
        {
            inputBuffer.Enqueue(input);

            // Limit buffer size
            while (inputBuffer.Count > 10)
            {
                inputBuffer.Dequeue();
            }
        }

        private void ProcessInputBuffer()
        {
            if (!enableInputBuffering) return;

            float currentTime = Time.time;

            // Remove old buffered inputs
            while (inputBuffer.Count > 0)
            {
                BufferedInput input = inputBuffer.Peek();

                if (currentTime - input.timestamp > inputBufferTime)
                {
                    inputBuffer.Dequeue();
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessKeyRepeats()
        {
            List<KeyCode> keys = new List<KeyCode>(keyHoldTimers.Keys);

            foreach (KeyCode key in keys)
            {
                if (!GetKey(key))
                {
                    keyHoldTimers.Remove(key);
                    keyRepeatTimers.Remove(key);
                    continue;
                }

                keyHoldTimers[key] += Time.deltaTime;

                if (keyHoldTimers[key] >= repeatDelay)
                {
                    if (!keyRepeatTimers.ContainsKey(key))
                    {
                        keyRepeatTimers[key] = 0f;
                    }

                    keyRepeatTimers[key] += Time.deltaTime;

                    if (keyRepeatTimers[key] >= repeatRate)
                    {
                        keyRepeatTimers[key] = 0f;
                        // Trigger repeat action
                        HandleKeyRepeat(key);
                    }
                }
            }
        }

        private void HandleKeyRepeat(KeyCode key)
        {
            // Handle specific keys that should repeat
            switch (key)
            {
                case KeyCode.W:
                case KeyCode.S:
                case KeyCode.A:
                case KeyCode.D:
                case KeyCode.UpArrow:
                case KeyCode.DownArrow:
                case KeyCode.LeftArrow:
                case KeyCode.RightArrow:
                    GatherMovementInput();
                    break;
            }
        }

        private bool GetKey(KeyCode key)
        {
            bool isPressed = UnityEngine.Input.GetKey(key);

            if (isPressed && !keyHoldTimers.ContainsKey(key))
            {
                keyHoldTimers[key] = 0f;
            }

            return isPressed;
        }

        private bool GetKeyDown(KeyCode key)
        {
            return UnityEngine.Input.GetKeyDown(key);
        }

        private bool GetKeyUp(KeyCode key)
        {
            return UnityEngine.Input.GetKeyUp(key);
        }

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    EnableInput();
                    break;
                case GameState.Paused:
                case GameState.GameOver:
                    DisableInput();
                    break;
            }
        }

        /// <summary>
        /// Enables input processing
        /// </summary>
        public void EnableInput()
        {
            inputEnabled = true;
            ClearInput();
        }

        /// <summary>
        /// Disables input processing
        /// </summary>
        public void DisableInput()
        {
            inputEnabled = false;
            ClearInput();
        }

        /// <summary>
        /// Clears all current input states
        /// </summary>
        public void ClearInput()
        {
            currentMovement = Vector3.zero;
            currentRotation = Vector3.zero;
            isHardDropping = false;
            isSoftDropping = false;
            isDragging = false;
            inputBuffer.Clear();
            keyHoldTimers.Clear();
            keyRepeatTimers.Clear();
        }

        /// <summary>
        /// Gets the current movement input
        /// </summary>
        public Vector3 GetMovementInput() => currentMovement;

        /// <summary>
        /// Gets the current rotation input
        /// </summary>
        public Vector3 GetRotationInput() => currentRotation;

        /// <summary>
        /// Gets whether hard drop is active
        /// </summary>
        public bool IsHardDropping() => isHardDropping;

        /// <summary>
        /// Gets whether soft drop is active
        /// </summary>
        public bool IsSoftDropping() => isSoftDropping;

        private enum InputType
        {
            Movement,
            Rotation,
            HardDrop,
            SoftDrop,
            Pause
        }

        private struct BufferedInput
        {
            public InputType type;
            public Vector3 value;
            public float timestamp;
        }
    }
}