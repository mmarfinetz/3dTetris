using UnityEngine;

namespace TetrisJenga.Physics
{
    /// <summary>
    /// Configuration for physics settings
    /// </summary>
    [CreateAssetMenu(fileName = "PhysicsConfig", menuName = "TetrisJenga/Physics Config")]
    public class PhysicsConfig : ScriptableObject
    {
        [Header("Global Physics")]
        [SerializeField] private float gravity = 9.81f;
        [SerializeField] private float fixedTimestep = 0.02f;
        [SerializeField] private int solverIterations = 10;
        [SerializeField] private int solverVelocityIterations = 2;

        [Header("Material Properties")]
        [SerializeField] private float defaultFriction = 0.6f;
        [SerializeField] private float defaultBounciness = 0.1f;
        [SerializeField] private PhysicsMaterialCombine frictionCombine = PhysicsMaterialCombine.Average;
        [SerializeField] private PhysicsMaterialCombine bounceCombine = PhysicsMaterialCombine.Average;

        [Header("Collision Detection")]
        [SerializeField] private float defaultContactOffset = 0.01f;
        [SerializeField] private float sleepThreshold = 0.005f;
        [SerializeField] private float bounceThreshold = 2f;
        [SerializeField] private float maxAngularVelocity = 50f;

        [Header("Performance")]
        [SerializeField] private bool autoSyncTransforms = false;
        [SerializeField] private bool reuseCollisionCallbacks = true;
        [SerializeField] private int maxSubsteps = 4;

        /// <summary>
        /// Applies physics configuration to Unity settings
        /// </summary>
        public void ApplyConfiguration()
        {
            // Set gravity
            UnityEngine.Physics.gravity = new Vector3(0, -gravity, 0);

            // Set timestep
            Time.fixedDeltaTime = fixedTimestep;

            // Set solver iterations
            UnityEngine.Physics.defaultSolverIterations = solverIterations;
            UnityEngine.Physics.defaultSolverVelocityIterations = solverVelocityIterations;

            // Set other physics properties
            UnityEngine.Physics.defaultContactOffset = defaultContactOffset;
            UnityEngine.Physics.sleepThreshold = sleepThreshold;
            UnityEngine.Physics.bounceThreshold = bounceThreshold;
            UnityEngine.Physics.defaultMaxAngularSpeed = maxAngularVelocity;

            // Performance settings
            UnityEngine.Physics.autoSyncTransforms = autoSyncTransforms;
            UnityEngine.Physics.reuseCollisionCallbacks = reuseCollisionCallbacks;

            UnityEngine.Debug.Log("Physics configuration applied successfully");
        }

        /// <summary>
        /// Creates a physics material with default settings
        /// </summary>
        public PhysicsMaterial CreateDefaultMaterial(string name = "DefaultPhysicsMaterial")
        {
            PhysicsMaterial material = new PhysicsMaterial(name);
            material.dynamicFriction = defaultFriction;
            material.staticFriction = defaultFriction * 1.2f;
            material.bounciness = defaultBounciness;
            material.frictionCombine = frictionCombine;
            material.bounceCombine = bounceCombine;
            return material;
        }

        /// <summary>
        /// Validates the configuration values
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (gravity < 0)
            {
                UnityEngine.Debug.LogError("Gravity must be positive (direction is handled by vector)");
                isValid = false;
            }

            if (fixedTimestep <= 0 || fixedTimestep > 0.1f)
            {
                UnityEngine.Debug.LogError("Fixed timestep must be between 0 and 0.1");
                isValid = false;
            }

            if (solverIterations < 1 || solverIterations > 50)
            {
                UnityEngine.Debug.LogError("Solver iterations must be between 1 and 50");
                isValid = false;
            }

            return isValid;
        }

        // Getters
        public float Gravity => gravity;
        public float FixedTimestep => fixedTimestep;
        public int SolverIterations => solverIterations;
        public float DefaultFriction => defaultFriction;
        public float DefaultBounciness => defaultBounciness;
    }
}