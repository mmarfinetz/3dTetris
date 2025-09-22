using System;
using System.Collections.Generic;
using UnityEngine;

namespace TetrisJenga.Pieces
{
    /// <summary>
    /// ScriptableObject that defines a piece type and its properties
    /// </summary>
    [CreateAssetMenu(fileName = "PieceDefinition", menuName = "TetrisJenga/Piece Definition")]
    public class PieceDefinition : ScriptableObject
    {
        [Header("Basic Properties")]
        [SerializeField] private string pieceName = "New Piece";
        [SerializeField] private PieceType pieceType = PieceType.Cube;
        [SerializeField] private Color pieceColor = Color.white;
        [SerializeField] private float massPerUnit = 1f;

        [Header("Shape Configuration")]
        [SerializeField] private List<Vector3> blockPositions = new List<Vector3>();
        [SerializeField] private Vector3 centerOfMass = Vector3.zero;
        [SerializeField] private Vector3 rotationPivot = Vector3.zero;

        [Header("Physics Properties")]
        [SerializeField] private float friction = 0.6f;
        [SerializeField] private float bounciness = 0.1f;
        [SerializeField] private float drag = 0.1f;
        [SerializeField] private float angularDrag = 0.5f;

        [Header("Scoring")]
        [SerializeField] private int basePoints = 100;
        [SerializeField] private float difficultyMultiplier = 1f;

        // Properties
        public string PieceName => pieceName;
        public PieceType Type => pieceType;
        public Color Color => pieceColor;
        public float MassPerUnit => massPerUnit;
        public List<Vector3> BlockPositions => new List<Vector3>(blockPositions);
        public Vector3 CenterOfMass => centerOfMass;
        public Vector3 RotationPivot => rotationPivot;
        public float Friction => friction;
        public float Bounciness => bounciness;
        public float Drag => drag;
        public float AngularDrag => angularDrag;
        public int BasePoints => basePoints;
        public float DifficultyMultiplier => difficultyMultiplier;

        /// <summary>
        /// Creates a game object from this piece definition
        /// </summary>
        public GameObject CreatePiece()
        {
            GameObject pieceObject = new GameObject(pieceName);
            pieceObject.tag = Core.Constants.TAG_PIECE;
            pieceObject.layer = Core.Constants.LAYER_PIECE;

            // Add rigidbody
            Rigidbody rb = pieceObject.AddComponent<Rigidbody>();
            rb.mass = blockPositions.Count * massPerUnit;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
            rb.centerOfMass = centerOfMass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Create physics material
            PhysicsMaterial physicMat = new PhysicsMaterial($"{pieceName}_PhysMat");
            physicMat.dynamicFriction = friction;
            physicMat.staticFriction = friction * 1.2f;
            physicMat.bounciness = bounciness;
            physicMat.frictionCombine = PhysicsMaterialCombine.Average;
            physicMat.bounceCombine = PhysicsMaterialCombine.Average;

            // Create blocks
            foreach (Vector3 blockPos in blockPositions)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = "Block";
                block.transform.SetParent(pieceObject.transform);
                block.transform.localPosition = blockPos;
                block.transform.localScale = Vector3.one;

                // Apply physics material
                Collider collider = block.GetComponent<Collider>();
                collider.material = physicMat;

                // Apply color
                Renderer renderer = block.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = pieceColor;
                    renderer.material = mat;
                }
            }

            // Add piece identifier component
            PieceIdentifier identifier = pieceObject.AddComponent<PieceIdentifier>();
            identifier.Initialize(this);

            return pieceObject;
        }

        /// <summary>
        /// Validates the piece definition
        /// </summary>
        public bool Validate()
        {
            if (blockPositions == null || blockPositions.Count == 0)
            {
                UnityEngine.Debug.LogError($"Piece {pieceName} has no blocks defined!");
                return false;
            }

            if (massPerUnit <= 0)
            {
                UnityEngine.Debug.LogError($"Piece {pieceName} has invalid mass!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Auto-calculates center of mass based on block positions
        /// </summary>
        [ContextMenu("Calculate Center of Mass")]
        public void CalculateCenterOfMass()
        {
            if (blockPositions == null || blockPositions.Count == 0)
            {
                centerOfMass = Vector3.zero;
                return;
            }

            Vector3 sum = Vector3.zero;
            foreach (Vector3 pos in blockPositions)
            {
                sum += pos;
            }
            centerOfMass = sum / blockPositions.Count;
        }

        /// <summary>
        /// Creates default piece definitions
        /// </summary>
        public static List<PieceDefinition> CreateDefaultPieces()
        {
            List<PieceDefinition> pieces = new List<PieceDefinition>();

            // Cube (1x1x1)
            PieceDefinition cube = CreateInstance<PieceDefinition>();
            cube.pieceName = "Cube";
            cube.pieceType = PieceType.Cube;
            cube.pieceColor = Core.Constants.COLOR_CUBE;
            cube.blockPositions = new List<Vector3> { Vector3.zero };
            cube.basePoints = 100;
            cube.difficultyMultiplier = 0.8f;
            cube.CalculateCenterOfMass();
            pieces.Add(cube);

            // Domino (1x2x1)
            PieceDefinition domino = CreateInstance<PieceDefinition>();
            domino.pieceName = "Domino";
            domino.pieceType = PieceType.Domino;
            domino.pieceColor = Core.Constants.COLOR_DOMINO;
            domino.blockPositions = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0)
            };
            domino.basePoints = 150;
            domino.difficultyMultiplier = 1f;
            domino.CalculateCenterOfMass();
            pieces.Add(domino);

            // Bar (1x3x1)
            PieceDefinition bar = CreateInstance<PieceDefinition>();
            bar.pieceName = "Bar";
            bar.pieceType = PieceType.Bar;
            bar.pieceColor = Core.Constants.COLOR_BAR;
            bar.blockPositions = new List<Vector3>
            {
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0)
            };
            bar.basePoints = 200;
            bar.difficultyMultiplier = 1.2f;
            bar.CalculateCenterOfMass();
            pieces.Add(bar);

            // L-Shape
            PieceDefinition lShape = CreateInstance<PieceDefinition>();
            lShape.pieceName = "L-Shape";
            lShape.pieceType = PieceType.LShape;
            lShape.pieceColor = Core.Constants.COLOR_L_SHAPE;
            lShape.blockPositions = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(1, 0, 0)
            };
            lShape.basePoints = 250;
            lShape.difficultyMultiplier = 1.5f;
            lShape.CalculateCenterOfMass();
            pieces.Add(lShape);

            // T-Shape
            PieceDefinition tShape = CreateInstance<PieceDefinition>();
            tShape.pieceName = "T-Shape";
            tShape.pieceType = PieceType.TShape;
            tShape.pieceColor = Core.Constants.COLOR_T_SHAPE;
            tShape.blockPositions = new List<Vector3>
            {
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 0, 1)
            };
            tShape.basePoints = 300;
            tShape.difficultyMultiplier = 1.8f;
            tShape.CalculateCenterOfMass();
            pieces.Add(tShape);

            return pieces;
        }
    }

    /// <summary>
    /// Enum defining piece types
    /// </summary>
    public enum PieceType
    {
        Cube,
        Domino,
        Bar,
        LShape,
        TShape,
        Custom
    }

    /// <summary>
    /// Component to identify piece type at runtime
    /// </summary>
    public class PieceIdentifier : MonoBehaviour
    {
        private PieceDefinition definition;
        private float placementTime;

        public PieceDefinition Definition => definition;
        public float PlacementTime => placementTime;

        public void Initialize(PieceDefinition def)
        {
            definition = def;
            placementTime = Time.time;
        }
    }
}