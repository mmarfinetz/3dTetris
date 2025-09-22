using System.Collections;
using UnityEngine;

namespace TetrisJenga.Pieces
{
    /// <summary>
    /// Controls the ghost piece preview behavior
    /// </summary>
    public class GhostPiece : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.2f;

        private Material[] materials;
        private float baseAlpha;
        private bool isFadingOut = false;
        private Coroutine fadeCoroutine;
        private Coroutine pulseCoroutine;

        private void Start()
        {
            CacheMaterials();
            StartPulse();
        }

        private void CacheMaterials()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            materials = new Material[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                materials[i] = renderers[i].material;
            }

            if (materials.Length > 0)
            {
                baseAlpha = materials[0].color.a;
            }
        }

        /// <summary>
        /// Starts the pulse animation
        /// </summary>
        private void StartPulse()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }

        private IEnumerator PulseAnimation()
        {
            while (!isFadingOut)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                float targetAlpha = baseAlpha + pulse;

                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        Color color = mat.color;
                        color.a = targetAlpha;
                        mat.color = color;
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Starts the fade out animation
        /// </summary>
        public void StartFadeOut()
        {
            if (isFadingOut) return;

            isFadingOut = true;

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeOutAnimation());
        }

        private IEnumerator FadeOutAnimation()
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeOutDuration;
                float alpha = baseAlpha * fadeOutCurve.Evaluate(t);

                foreach (Material mat in materials)
                {
                    if (mat != null)
                    {
                        Color color = mat.color;
                        color.a = alpha;
                        mat.color = color;
                    }
                }

                yield return null;
            }

            // Ensure fully transparent
            foreach (Material mat in materials)
            {
                if (mat != null)
                {
                    Color color = mat.color;
                    color.a = 0f;
                    mat.color = color;
                }
            }
        }

        /// <summary>
        /// Updates the ghost position smoothly
        /// </summary>
        public void UpdatePosition(Vector3 targetPosition, Quaternion targetRotation)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        /// <summary>
        /// Shows or hides the ghost piece
        /// </summary>
        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
        }

        private void OnDestroy()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }
        }
    }
}