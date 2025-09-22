using UnityEngine;
using UnityEngine.UI;
using TetrisJenga.Core;

namespace TetrisJenga.UI
{
    /// <summary>
    /// Visual stability indicator gauge
    /// </summary>
    public class StabilityGauge : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text stabilityText;
        [SerializeField] private Image warningIcon;

        [Header("Colors")]
        [SerializeField] private Gradient stabilityGradient;
        [SerializeField] private Color pulseColor = Color.red;

        [Header("Animation")]
        [SerializeField] private float updateSpeed = 5f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // State
        private float currentStability = 100f;
        private float targetStability = 100f;
        private float pulseTimer = 0f;
        private bool isPulsing = false;

        private void Awake()
        {
            // Ensure we have a RectTransform
            if (GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
            }

            CreateGaugeIfNeeded();
        }

        private void CreateGaugeIfNeeded()
        {
            if (fillImage == null)
            {
                // Create background
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(transform);
                backgroundImage = bgObj.AddComponent<Image>();
                backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
                RectTransform bgRect = bgObj.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;

                // Create fill
                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(transform);
                fillImage = fillObj.AddComponent<Image>();
                fillImage.color = Color.green;
                fillImage.type = Image.Type.Filled;
                fillImage.fillMethod = Image.FillMethod.Horizontal;
                fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
                RectTransform fillRect = fillObj.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = new Vector2(2, 2);
                fillRect.offsetMax = new Vector2(-2, -2);

                // Create text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(transform);
                stabilityText = textObj.AddComponent<Text>();
                stabilityText.text = "STABILITY: 100%";
                stabilityText.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                stabilityText.fontSize = 14;
                stabilityText.color = Color.white;
                stabilityText.alignment = TextAnchor.MiddleCenter;
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                // Create warning icon
                GameObject iconObj = new GameObject("WarningIcon");
                iconObj.transform.SetParent(transform);
                warningIcon = iconObj.AddComponent<Image>();
                warningIcon.color = Color.red;
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(1, 0.5f);
                iconRect.anchorMax = new Vector2(1, 0.5f);
                iconRect.pivot = new Vector2(1, 0.5f);
                iconRect.sizeDelta = new Vector2(30, 30);
                iconRect.anchoredPosition = new Vector2(-5, 0);
                warningIcon.gameObject.SetActive(false);
            }

            // Create default gradient if not set
            if (stabilityGradient == null || stabilityGradient.colorKeys.Length == 0)
            {
                stabilityGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[]
                {
                    new GradientColorKey(Constants.COLOR_STABILITY_CRITICAL, 0f),
                    new GradientColorKey(Constants.COLOR_STABILITY_WARNING, 0.3f),
                    new GradientColorKey(Constants.COLOR_STABILITY_GOOD, 1f)
                };
                GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                };
                stabilityGradient.SetKeys(colorKeys, alphaKeys);
            }
        }

        private void Update()
        {
            // Smooth stability update
            currentStability = Mathf.Lerp(currentStability, targetStability, Time.deltaTime * updateSpeed);

            // Update fill
            if (fillImage != null)
            {
                fillImage.fillAmount = currentStability / 100f;
                fillImage.color = stabilityGradient.Evaluate(currentStability / 100f);
            }

            // Update text
            if (stabilityText != null)
            {
                stabilityText.text = $"STABILITY: {Mathf.RoundToInt(currentStability)}%";
            }

            // Handle pulsing
            if (isPulsing)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulse = pulseCurve.Evaluate(Mathf.PingPong(pulseTimer, 1f));

                if (fillImage != null)
                {
                    Color currentColor = fillImage.color;
                    fillImage.color = Color.Lerp(currentColor, pulseColor, pulse * 0.5f);
                }

                if (warningIcon != null)
                {
                    warningIcon.color = new Color(1f, 0f, 0f, pulse);
                }
            }

            // Check for warning states
            UpdateWarningState();
        }

        private void UpdateWarningState()
        {
            bool shouldPulse = currentStability < Constants.STABILITY_CRITICAL_THRESHOLD;
            bool shouldWarn = currentStability < Constants.STABILITY_WARNING_THRESHOLD;

            if (shouldPulse != isPulsing)
            {
                isPulsing = shouldPulse;
                pulseTimer = 0f;
            }

            if (warningIcon != null)
            {
                warningIcon.gameObject.SetActive(shouldWarn);
            }

            // Add screen shake or other effects
            if (shouldPulse)
            {
                // Could trigger camera shake here
                Camera.main?.GetComponent<Input.CameraController>()?.TriggerShake(0.1f, 0.1f);
            }
        }

        /// <summary>
        /// Sets the stability value
        /// </summary>
        public void SetStability(float stability)
        {
            targetStability = Mathf.Clamp(stability, 0f, 100f);
        }

        /// <summary>
        /// Immediately sets the stability without animation
        /// </summary>
        public void SetStabilityImmediate(float stability)
        {
            targetStability = Mathf.Clamp(stability, 0f, 100f);
            currentStability = targetStability;

            if (fillImage != null)
            {
                fillImage.fillAmount = currentStability / 100f;
                fillImage.color = stabilityGradient.Evaluate(currentStability / 100f);
            }

            if (stabilityText != null)
            {
                stabilityText.text = $"STABILITY: {Mathf.RoundToInt(currentStability)}%";
            }
        }

        /// <summary>
        /// Triggers a warning animation
        /// </summary>
        public void TriggerWarning()
        {
            isPulsing = true;
            pulseTimer = 0f;

            // Flash warning for a few seconds
            Invoke(nameof(StopWarning), 3f);
        }

        private void StopWarning()
        {
            if (currentStability >= Constants.STABILITY_WARNING_THRESHOLD)
            {
                isPulsing = false;
            }
        }
    }
}