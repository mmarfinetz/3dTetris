using UnityEngine;
using System.Runtime.InteropServices;

namespace TetrisJenga.WebGL
{
    /// <summary>
    /// Helper class for WebGL-specific functionality
    /// </summary>
    public class WebGLHelper : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern bool IsMobile();

        [DllImport("__Internal")]
        private static extern void SetCanvasSize(int width, int height);

        [DllImport("__Internal")]
        private static extern void ShowAlert(string message);
#endif

        private static WebGLHelper instance;

        public static bool IsWebGLBuild
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsRunningOnMobile
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                try
                {
                    return IsMobile();
                }
                catch
                {
                    return false;
                }
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                if (IsWebGLBuild)
                {
                    InitializeWebGL();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeWebGL()
        {
            // Optimize for WebGL
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 1;

            // Adjust quality for mobile
            if (IsRunningOnMobile)
            {
                QualitySettings.SetQualityLevel(1); // Lower quality for mobile
                Screen.SetResolution(960, 540, true);
            }
            else
            {
                QualitySettings.SetQualityLevel(2); // Medium quality for desktop
            }

            Debug.Log($"WebGL initialized - Mobile: {IsRunningOnMobile}");
        }

        public static void ResizeCanvas(int width, int height)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            SetCanvasSize(width, height);
#endif
        }

        public static void ShowMessage(string message)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ShowAlert(message);
#else
            Debug.Log($"WebGL Message: {message}");
#endif
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (IsWebGLBuild)
            {
                // Pause/resume based on focus
                if (!hasFocus)
                {
                    if (Core.GameManager.Instance?.GetCurrentState() == Core.GameState.Playing)
                    {
                        Core.GameManager.Instance.PauseGame();
                    }
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (IsWebGLBuild)
            {
                if (pauseStatus)
                {
                    if (Core.GameManager.Instance?.GetCurrentState() == Core.GameState.Playing)
                    {
                        Core.GameManager.Instance.PauseGame();
                    }
                }
            }
        }
    }
}