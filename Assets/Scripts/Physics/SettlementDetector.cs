using System.Collections.Generic;
using UnityEngine;
using TetrisJenga.Core;

namespace TetrisJenga.Physics
{
    /// <summary>
    /// Detects when pieces and the tower have settled
    /// </summary>
    public class SettlementDetector : MonoBehaviour
    {
        [Header("Settlement Thresholds")]
        [SerializeField] private float velocityThreshold = 0.006f;
        [SerializeField] private float angularVelocityThreshold = 0.008f;
        [SerializeField] private float settlementTime = 0.35f;
        [SerializeField] private float microAdjustmentThreshold = 0.003f;

        [Header("Detection Settings")]
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private bool enableMicroAdjustments = true;

        // Tracking
        private Dictionary<GameObject, PieceSettlementInfo> pieceInfoMap = new Dictionary<GameObject, PieceSettlementInfo>();
        private float towerSettlementTimer = 0f;
        private bool isTowerSettled = true;
        private float nextCheckTime = 0f;

        // Events
        public delegate void PieceSettledHandler(GameObject piece);
        public delegate void TowerSettledHandler();
        public event PieceSettledHandler OnPieceSettled;
        public event TowerSettledHandler OnTowerSettled;

        private GameManager gameManager;

        private void Start()
        {
            gameManager = GameManager.Instance;
        }

        private void Update()
        {
            if (Time.time >= nextCheckTime)
            {
                CheckSettlement();
                nextCheckTime = Time.time + checkInterval;
            }
        }

        private void CheckSettlement()
        {
            List<GameObject> activePieces = gameManager?.GetActivePieces() ?? new List<GameObject>();

            // Update piece tracking
            UpdatePieceTracking(activePieces);

            // Check individual pieces
            bool allPiecesSettled = true;
            foreach (var kvp in pieceInfoMap)
            {
                if (kvp.Key == null) continue;

                PieceSettlementInfo info = kvp.Value;
                bool isSettled = CheckPieceSettlement(kvp.Key, ref info);

                if (!info.isSettled && isSettled)
                {
                    // Piece just settled
                    info.isSettled = true;
                    info.settledTime = Time.time;
                    OnPieceSettled?.Invoke(kvp.Key);

                    // Apply micro adjustments
                    if (enableMicroAdjustments)
                    {
                        ApplyMicroAdjustments(kvp.Key);
                    }
                }

                pieceInfoMap[kvp.Key] = info;

                if (!info.isSettled)
                {
                    allPiecesSettled = false;
                }
            }

            // Check tower settlement
            if (allPiecesSettled && !isTowerSettled)
            {
                towerSettlementTimer += checkInterval;

                if (towerSettlementTimer >= settlementTime)
                {
                    isTowerSettled = true;
                    OnTowerSettled?.Invoke();
                    UnityEngine.Debug.Log("Tower has settled!");
                }
            }
            else if (!allPiecesSettled)
            {
                towerSettlementTimer = 0f;
                isTowerSettled = false;
            }
        }

        private void UpdatePieceTracking(List<GameObject> activePieces)
        {
            // Add new pieces
            foreach (GameObject piece in activePieces)
            {
                if (piece != null && !pieceInfoMap.ContainsKey(piece))
                {
                    pieceInfoMap[piece] = new PieceSettlementInfo
                    {
                        isSettled = false,
                        settledTime = 0f,
                        lastPosition = piece.transform.position,
                        lastRotation = piece.transform.rotation,
                        settlementTimer = 0f
                    };
                }
            }

            // Remove destroyed pieces
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var kvp in pieceInfoMap)
            {
                if (kvp.Key == null || !activePieces.Contains(kvp.Key))
                {
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (GameObject piece in toRemove)
            {
                pieceInfoMap.Remove(piece);
            }
        }

        private bool CheckPieceSettlement(GameObject piece, ref PieceSettlementInfo info)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null) return true;

            // Check velocity
            bool velocitySettled = rb.linearVelocity.magnitude < velocityThreshold;
            bool angularSettled = rb.angularVelocity.magnitude < angularVelocityThreshold;

            // Check position/rotation change
            float positionDelta = Vector3.Distance(piece.transform.position, info.lastPosition);
            float rotationDelta = Quaternion.Angle(piece.transform.rotation, info.lastRotation);

            bool positionSettled = positionDelta < microAdjustmentThreshold;
            bool rotationSettled = rotationDelta < 0.1f;

            // Update tracking
            info.lastPosition = piece.transform.position;
            info.lastRotation = piece.transform.rotation;

            // Check if all conditions are met
            bool isCurrentlySettled = velocitySettled && angularSettled && positionSettled && rotationSettled;

            if (isCurrentlySettled)
            {
                info.settlementTimer += checkInterval;
                return info.settlementTimer >= settlementTime;
            }
            else
            {
                info.settlementTimer = 0f;
                return false;
            }
        }

        private void ApplyMicroAdjustments(GameObject piece)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null) return;

            // Apply slight damping to prevent micro-movements
            rb.linearVelocity *= 0.85f;
            rb.angularVelocity *= 0.85f;

            // Snap to grid if very close
            Vector3 pos = piece.transform.position;
            float snapThreshold = 0.08f;

            if (Mathf.Abs(pos.x - Mathf.Round(pos.x)) < snapThreshold)
            {
                pos.x = Mathf.Round(pos.x);
            }
            if (Mathf.Abs(pos.z - Mathf.Round(pos.z)) < snapThreshold)
            {
                pos.z = Mathf.Round(pos.z);
            }

            rb.MovePosition(pos);
        }

        /// <summary>
        /// Forces a settlement check
        /// </summary>
        public void ForceCheck()
        {
            CheckSettlement();
        }

        /// <summary>
        /// Resets all settlement tracking
        /// </summary>
        public void Reset()
        {
            pieceInfoMap.Clear();
            towerSettlementTimer = 0f;
            isTowerSettled = true;
        }

        /// <summary>
        /// Gets whether the tower is currently settled
        /// </summary>
        public bool IsTowerSettled()
        {
            return isTowerSettled;
        }

        /// <summary>
        /// Gets settlement info for a specific piece
        /// </summary>
        public bool IsPieceSettled(GameObject piece)
        {
            if (pieceInfoMap.ContainsKey(piece))
            {
                return pieceInfoMap[piece].isSettled;
            }
            return false;
        }

        /// <summary>
        /// Gets detailed settlement statistics
        /// </summary>
        public SettlementStats GetStats()
        {
            int totalPieces = pieceInfoMap.Count;
            int settledPieces = 0;
            float avgSettlementTime = 0f;

            foreach (var info in pieceInfoMap.Values)
            {
                if (info.isSettled)
                {
                    settledPieces++;
                    avgSettlementTime += info.settledTime;
                }
            }

            if (settledPieces > 0)
            {
                avgSettlementTime /= settledPieces;
            }

            return new SettlementStats
            {
                totalPieces = totalPieces,
                settledPieces = settledPieces,
                settlementPercentage = totalPieces > 0 ? (float)settledPieces / totalPieces * 100f : 100f,
                averageSettlementTime = avgSettlementTime,
                isTowerSettled = isTowerSettled
            };
        }

        private struct PieceSettlementInfo
        {
            public bool isSettled;
            public float settledTime;
            public Vector3 lastPosition;
            public Quaternion lastRotation;
            public float settlementTimer;
        }
    }

    /// <summary>
    /// Settlement statistics
    /// </summary>
    [System.Serializable]
    public struct SettlementStats
    {
        public int totalPieces;
        public int settledPieces;
        public float settlementPercentage;
        public float averageSettlementTime;
        public bool isTowerSettled;
    }
}