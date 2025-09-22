using UnityEngine;

namespace TetrisJenga.Core
{
    /// <summary>
    /// Global constants and configuration values
    /// </summary>
    public static class Constants
    {
        // Physics
        public const float GRAVITY = 9.81f;
        public const float DEFAULT_MASS_PER_UNIT = 1f;
        public const float DEFAULT_FRICTION = 0.6f;
        public const float DEFAULT_BOUNCINESS = 0.1f;
        public const float SETTLEMENT_VELOCITY_THRESHOLD = 0.01f;
        public const float SETTLEMENT_ANGULAR_VELOCITY_THRESHOLD = 0.01f;

        // Gameplay
        public const float PIECE_SPAWN_HEIGHT = 15f;
        public const float GHOST_PREVIEW_ALPHA = 0.3f;
        public const float HARD_DROP_SPEED = 50f;
        public const float SOFT_DROP_SPEED = 10f;
        public const float NORMAL_DROP_SPEED = 1f;

        // Input
        public const float CAMERA_ROTATION_SPEED = 100f;
        public const float CAMERA_ZOOM_SPEED = 5f;
        public const float CAMERA_MIN_DISTANCE = 5f;
        public const float CAMERA_MAX_DISTANCE = 30f;
        public const float PIECE_MOVEMENT_SPEED = 5f;
        public const float PIECE_ROTATION_SPEED = 90f;

        // UI
        public const float UI_ANIMATION_DURATION = 0.3f;
        public const float STABILITY_WARNING_THRESHOLD = 30f;
        public const float STABILITY_CRITICAL_THRESHOLD = 15f;

        // Colors
        public static readonly Color COLOR_CUBE = new Color(0.2f, 0.6f, 1f);
        public static readonly Color COLOR_DOMINO = new Color(1f, 0.6f, 0.2f);
        public static readonly Color COLOR_BAR = new Color(0.2f, 1f, 0.6f);
        public static readonly Color COLOR_L_SHAPE = new Color(1f, 0.2f, 0.6f);
        public static readonly Color COLOR_T_SHAPE = new Color(0.6f, 0.2f, 1f);

        public static readonly Color COLOR_STABILITY_GOOD = new Color(0.2f, 1f, 0.2f);
        public static readonly Color COLOR_STABILITY_WARNING = new Color(1f, 1f, 0.2f);
        public static readonly Color COLOR_STABILITY_CRITICAL = new Color(1f, 0.2f, 0.2f);

        // Layers
        public const int LAYER_DEFAULT = 0;
        public const int LAYER_PIECE = 8;
        public const int LAYER_GHOST = 9;
        public const int LAYER_UI = 10;

        // Tags
        public const string TAG_PIECE = "Piece";
        public const string TAG_BASE = "Base";
        public const string TAG_BOUNDARY = "Boundary";
    }
}