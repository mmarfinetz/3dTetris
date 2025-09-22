using UnityEngine;
using TetrisJenga.Core;

namespace TetrisJenga.Input
{
    /// <summary>
    /// Controls camera orbit and zoom around the tower
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Orbit Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float orbitSpeed = 100f;
        [SerializeField] private float orbitDamping = 5f;
        [SerializeField] private float minVerticalAngle = 10f;
        [SerializeField] private float maxVerticalAngle = 80f;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 30f;
        [SerializeField] private float zoomDamping = 5f;

        [Header("Focus Settings")]
        [SerializeField] private bool autoFocus = true;
        [SerializeField] private float focusSpeed = 2f;
        [SerializeField] private Vector3 focusOffset = new Vector3(0, 5, 0);

        [Header("Camera Shake")]
        [SerializeField] private bool enableShake = true;
        [SerializeField] private float shakeIntensity = 0.1f;
        [SerializeField] private float shakeFrequency = 10f;

        // Current state
        private float currentDistance = 15f;
        private float targetDistance = 15f;
        private float currentHorizontalAngle = 45f;
        private float currentVerticalAngle = 30f;
        private Vector3 currentFocusPoint;
        private Vector3 targetFocusPoint;

        // Shake
        private float shakeTimer = 0f;
        private float currentShakeIntensity = 0f;

        // Components
        private Camera cam;
        private InputManager inputManager;
        private GameManager gameManager;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = gameObject.AddComponent<Camera>();
            }

            // Set default target if not assigned
            if (target == null)
            {
                GameObject targetObj = GameObject.Find("TowerBase");
                if (targetObj == null)
                {
                    targetObj = new GameObject("CameraTarget");
                    targetObj.transform.position = Vector3.zero;
                }
                target = targetObj.transform;
            }

            currentFocusPoint = target.position + focusOffset;
            targetFocusPoint = currentFocusPoint;
        }

        private void Start()
        {
            inputManager = FindFirstObjectByType<InputManager>();
            gameManager = GameManager.Instance;

            // Subscribe to input events
            if (inputManager != null)
            {
                inputManager.OnCameraRotateInput.AddListener(OnCameraRotate);
                inputManager.OnCameraZoomInput.AddListener(OnCameraZoom);
            }

            // Subscribe to game events
            if (gameManager != null)
            {
                gameManager.OnStabilityChanged.AddListener(OnStabilityChanged);
            }

            // Set initial position
            UpdateCameraPosition();
        }

        private void LateUpdate()
        {
            // Update focus point
            if (autoFocus)
            {
                UpdateFocusPoint();
            }

            // Smooth focus point movement
            currentFocusPoint = Vector3.Lerp(currentFocusPoint, targetFocusPoint, Time.deltaTime * focusSpeed);

            // Smooth distance
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomDamping);

            // Update camera position
            UpdateCameraPosition();

            // Apply camera shake if needed
            if (enableShake && currentShakeIntensity > 0)
            {
                ApplyCameraShake();
            }
        }

        private void UpdateFocusPoint()
        {
            if (gameManager == null) return;

            // Focus on tower center of mass
            float towerHeight = gameManager.GetCurrentHeight();
            targetFocusPoint = target.position + new Vector3(0, towerHeight * 0.5f, 0);
        }

        private void UpdateCameraPosition()
        {
            // Calculate position based on spherical coordinates
            float radH = currentHorizontalAngle * Mathf.Deg2Rad;
            float radV = currentVerticalAngle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                Mathf.Sin(radH) * Mathf.Cos(radV),
                Mathf.Sin(radV),
                Mathf.Cos(radH) * Mathf.Cos(radV)
            ) * currentDistance;

            transform.position = currentFocusPoint + offset;
            transform.LookAt(currentFocusPoint);
        }

        private void OnCameraRotate(float deltaX)
        {
            currentHorizontalAngle += deltaX * orbitSpeed * Time.deltaTime;
            currentHorizontalAngle = Mathf.Repeat(currentHorizontalAngle, 360f);
        }

        private void OnCameraZoom(float delta)
        {
            targetDistance -= delta * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        /// <summary>
        /// Rotates camera vertically
        /// </summary>
        public void RotateVertical(float delta)
        {
            currentVerticalAngle += delta * orbitSpeed * Time.deltaTime;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        /// <summary>
        /// Sets camera focus to a specific point
        /// </summary>
        public void SetFocus(Vector3 point, bool immediate = false)
        {
            targetFocusPoint = point;

            if (immediate)
            {
                currentFocusPoint = targetFocusPoint;
                UpdateCameraPosition();
            }
        }

        /// <summary>
        /// Resets camera to default position
        /// </summary>
        public void ResetCamera()
        {
            currentHorizontalAngle = 45f;
            currentVerticalAngle = 30f;
            targetDistance = 15f;
            currentDistance = 15f;
            targetFocusPoint = target.position + focusOffset;
            currentFocusPoint = targetFocusPoint;
            UpdateCameraPosition();
        }

        private void OnStabilityChanged(float stability)
        {
            if (!enableShake) return;

            // Increase shake when stability is low
            if (stability < Constants.STABILITY_WARNING_THRESHOLD)
            {
                float instabilityFactor = 1f - (stability / Constants.STABILITY_WARNING_THRESHOLD);
                currentShakeIntensity = shakeIntensity * instabilityFactor;
            }
            else
            {
                currentShakeIntensity = 0f;
            }
        }

        private void ApplyCameraShake()
        {
            shakeTimer += Time.deltaTime * shakeFrequency;

            float shakeX = Mathf.PerlinNoise(shakeTimer, 0) - 0.5f;
            float shakeY = Mathf.PerlinNoise(0, shakeTimer) - 0.5f;

            Vector3 shakeOffset = new Vector3(shakeX, shakeY, 0) * currentShakeIntensity;
            transform.position += transform.TransformDirection(shakeOffset);
        }

        /// <summary>
        /// Triggers a one-time camera shake
        /// </summary>
        public void TriggerShake(float intensity, float duration)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            float originalIntensity = currentShakeIntensity;
            currentShakeIntensity = intensity;

            yield return new WaitForSeconds(duration);

            currentShakeIntensity = originalIntensity;
        }

        /// <summary>
        /// Gets the current camera distance
        /// </summary>
        public float GetDistance() => currentDistance;

        /// <summary>
        /// Sets the camera distance
        /// </summary>
        public void SetDistance(float distance)
        {
            targetDistance = Mathf.Clamp(distance, minDistance, maxDistance);
        }

        /// <summary>
        /// Enables or disables auto focus
        /// </summary>
        public void SetAutoFocus(bool enabled)
        {
            autoFocus = enabled;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (inputManager != null)
            {
                inputManager.OnCameraRotateInput.RemoveListener(OnCameraRotate);
                inputManager.OnCameraZoomInput.RemoveListener(OnCameraZoom);
            }

            if (gameManager != null)
            {
                gameManager.OnStabilityChanged.RemoveListener(OnStabilityChanged);
            }
        }
    }
}