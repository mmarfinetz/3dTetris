using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TetrisJenga.Core;

namespace TetrisJenga.Physics
{
    /// <summary>
    /// Analyzes tower stability using physics calculations
    /// </summary>
    public class StabilityAnalyzer : MonoBehaviour
    {
        [Header("Analysis Settings")]
        [SerializeField] private float analysisInterval = 0.1f;
        [SerializeField] private float stabilitySmoothing = 0.5f;
        [SerializeField] private float centerOfMassThreshold = 0.8f;
        [SerializeField] private float oscillationDampingFactor = 0.9f;

        [Header("Support Polygon")]
        [SerializeField] private float contactPointMergeDistance = 0.1f;
        [SerializeField] private int minContactPoints = 3;
        [SerializeField] private float supportPolygonPadding = 0.05f;

        [Header("Debug Visualization")]
        [SerializeField] private bool showDebugVisualization = true;
        [SerializeField] private Color centerOfMassColor = Color.yellow;
        [SerializeField] private Color supportPolygonColor = Color.green;
        [SerializeField] private Color unstableColor = Color.red;

        // Analysis results
        private float currentStability = 100f;
        private float targetStability = 100f;
        private Vector3 towerCenterOfMass;
        private List<Vector3> supportPolygonPoints = new List<Vector3>();
        private List<ContactPoint> activeContacts = new List<ContactPoint>();
        private float oscillationMagnitude = 0f;

        // Cached references
        private GameManager gameManager;
        private List<GameObject> analyzedPieces = new List<GameObject>();
        private float nextAnalysisTime = 0f;

        // History for oscillation detection
        private Queue<Vector3> centerOfMassHistory = new Queue<Vector3>();
        private const int HISTORY_SIZE = 20;

        private void Start()
        {
            gameManager = GameManager.Instance;
        }

        private void Update()
        {
            if (Time.time >= nextAnalysisTime)
            {
                PerformStabilityAnalysis();
                nextAnalysisTime = Time.time + analysisInterval;
            }

            // Smooth stability value
            currentStability = Mathf.Lerp(currentStability, targetStability, Time.deltaTime / stabilitySmoothing);

            // Update game manager
            if (gameManager != null)
            {
                gameManager.UpdateStability(currentStability);
            }
        }

        /// <summary>
        /// Performs comprehensive stability analysis
        /// </summary>
        private void PerformStabilityAnalysis()
        {
            // Get all active pieces
            analyzedPieces = gameManager?.GetActivePieces() ?? new List<GameObject>();

            if (analyzedPieces.Count == 0)
            {
                targetStability = 100f;
                return;
            }

            // Calculate center of mass
            towerCenterOfMass = CalculateCenterOfMass();

            // Track center of mass history
            UpdateCenterOfMassHistory(towerCenterOfMass);

            // Calculate support polygon
            CalculateSupportPolygon();

            // Analyze stability factors
            float massStability = AnalyzeCenterOfMassStability();
            float contactStability = AnalyzeContactStability();
            float oscillationStability = AnalyzeOscillationStability();
            float tiltStability = AnalyzeTiltStability();

            // Combine factors with weights
            targetStability = (massStability * 0.4f +
                             contactStability * 0.3f +
                             oscillationStability * 0.2f +
                             tiltStability * 0.1f);

            targetStability = Mathf.Clamp(targetStability, 0f, 100f);
        }

        /// <summary>
        /// Calculates the combined center of mass of all pieces
        /// </summary>
        private Vector3 CalculateCenterOfMass()
        {
            Vector3 weightedSum = Vector3.zero;
            float totalMass = 0f;

            foreach (GameObject piece in analyzedPieces)
            {
                if (piece != null)
                {
                    Rigidbody rb = piece.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        weightedSum += rb.worldCenterOfMass * rb.mass;
                        totalMass += rb.mass;
                    }
                }
            }

            return totalMass > 0 ? weightedSum / totalMass : Vector3.zero;
        }

        /// <summary>
        /// Calculates the support polygon from contact points
        /// </summary>
        private void CalculateSupportPolygon()
        {
            activeContacts.Clear();
            supportPolygonPoints.Clear();

            // Collect all contact points with the base or stable pieces
            foreach (GameObject piece in analyzedPieces)
            {
                if (piece == null) continue;

                Collider[] colliders = piece.GetComponentsInChildren<Collider>();
                foreach (Collider col in colliders)
                {
                    // Get contacts using Physics overlap
                    Collider[] overlaps = UnityEngine.Physics.OverlapBox(
                        col.bounds.center,
                        col.bounds.extents,
                        col.transform.rotation
                    );

                    foreach (Collider overlap in overlaps)
                    {
                        if (overlap == col) continue;

                        // Check if contact is with base or another piece
                        if (overlap.CompareTag(Constants.TAG_BASE) ||
                            overlap.CompareTag(Constants.TAG_PIECE))
                        {
                            // Approximate contact point
                            Vector3 contactPoint = col.ClosestPoint(overlap.bounds.center);
                            contactPoint.y = 0; // Project to ground plane

                            // Check if this point is unique
                            bool isUnique = true;
                            foreach (Vector3 existing in supportPolygonPoints)
                            {
                                if (Vector3.Distance(contactPoint, existing) < contactPointMergeDistance)
                                {
                                    isUnique = false;
                                    break;
                                }
                            }

                            if (isUnique)
                            {
                                supportPolygonPoints.Add(contactPoint);
                            }
                        }
                    }
                }
            }

            // Calculate convex hull if we have enough points
            if (supportPolygonPoints.Count >= minContactPoints)
            {
                supportPolygonPoints = CalculateConvexHull(supportPolygonPoints);
            }
        }

        /// <summary>
        /// Calculates convex hull of points using Graham scan
        /// </summary>
        private List<Vector3> CalculateConvexHull(List<Vector3> points)
        {
            if (points.Count < 3) return points;

            // Sort points by x-coordinate
            points = points.OrderBy(p => p.x).ThenBy(p => p.z).ToList();

            // Build lower hull
            List<Vector3> lower = new List<Vector3>();
            foreach (Vector3 p in points)
            {
                while (lower.Count >= 2 && CrossProduct2D(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                {
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(p);
            }

            // Build upper hull
            List<Vector3> upper = new List<Vector3>();
            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector3 p = points[i];
                while (upper.Count >= 2 && CrossProduct2D(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                {
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(p);
            }

            // Remove last point of each half because it's repeated
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);

            // Concatenate lower and upper hull
            lower.AddRange(upper);
            return lower;
        }

        private float CrossProduct2D(Vector3 a, Vector3 b, Vector3 c)
        {
            return (b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x);
        }

        /// <summary>
        /// Analyzes stability based on center of mass position
        /// </summary>
        private float AnalyzeCenterOfMassStability()
        {
            if (supportPolygonPoints.Count < minContactPoints)
            {
                return 50f; // No stable support polygon
            }

            // Project center of mass to ground plane
            Vector3 projectedCOM = new Vector3(towerCenterOfMass.x, 0, towerCenterOfMass.z);

            // Check if point is inside polygon
            bool isInside = IsPointInPolygon(projectedCOM, supportPolygonPoints);

            if (!isInside)
            {
                return 0f; // Center of mass outside support - unstable!
            }

            // Calculate distance to polygon edge
            float minDistance = float.MaxValue;
            for (int i = 0; i < supportPolygonPoints.Count; i++)
            {
                Vector3 p1 = supportPolygonPoints[i];
                Vector3 p2 = supportPolygonPoints[(i + 1) % supportPolygonPoints.Count];

                float distance = DistancePointToLineSegment(projectedCOM, p1, p2);
                minDistance = Mathf.Min(minDistance, distance);
            }

            // Calculate polygon radius
            float polygonRadius = 0f;
            Vector3 polygonCenter = Vector3.zero;
            foreach (Vector3 point in supportPolygonPoints)
            {
                polygonCenter += point;
            }
            polygonCenter /= supportPolygonPoints.Count;

            foreach (Vector3 point in supportPolygonPoints)
            {
                float dist = Vector3.Distance(point, polygonCenter);
                polygonRadius = Mathf.Max(polygonRadius, dist);
            }

            // Normalize distance to stability score
            float normalizedDistance = minDistance / polygonRadius;
            return Mathf.Clamp01(normalizedDistance / centerOfMassThreshold) * 100f;
        }

        /// <summary>
        /// Checks if a point is inside a polygon
        /// </summary>
        private bool IsPointInPolygon(Vector3 point, List<Vector3> polygon)
        {
            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; i++)
            {
                if ((polygon[i].z > point.z) != (polygon[j].z > point.z) &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.z - polygon[i].z) /
                    (polygon[j].z - polygon[i].z) + polygon[i].x)
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }

        private float DistancePointToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 line = lineEnd - lineStart;
            float lineLength = line.magnitude;
            line.Normalize();

            Vector3 toPoint = point - lineStart;
            float dot = Vector3.Dot(toPoint, line);

            if (dot <= 0)
            {
                return Vector3.Distance(point, lineStart);
            }
            else if (dot >= lineLength)
            {
                return Vector3.Distance(point, lineEnd);
            }
            else
            {
                Vector3 projection = lineStart + line * dot;
                return Vector3.Distance(point, projection);
            }
        }

        /// <summary>
        /// Analyzes stability based on contact quality
        /// </summary>
        private float AnalyzeContactStability()
        {
            if (supportPolygonPoints.Count < minContactPoints)
            {
                return 0f;
            }

            // Calculate polygon area
            float area = CalculatePolygonArea(supportPolygonPoints);

            // Larger support area = more stable
            float targetArea = analyzedPieces.Count * 2f; // Expected area based on piece count
            float areaRatio = Mathf.Clamp01(area / targetArea);

            // Check contact distribution
            float distribution = CalculateContactDistribution();

            return (areaRatio * 0.7f + distribution * 0.3f) * 100f;
        }

        private float CalculatePolygonArea(List<Vector3> polygon)
        {
            float area = 0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector3 p1 = polygon[i];
                Vector3 p2 = polygon[(i + 1) % polygon.Count];
                area += (p1.x * p2.z - p2.x * p1.z);
            }
            return Mathf.Abs(area) * 0.5f;
        }

        private float CalculateContactDistribution()
        {
            if (supportPolygonPoints.Count < 2) return 0f;

            // Calculate variance in contact point distances
            Vector3 center = Vector3.zero;
            foreach (Vector3 point in supportPolygonPoints)
            {
                center += point;
            }
            center /= supportPolygonPoints.Count;

            float avgDistance = 0f;
            foreach (Vector3 point in supportPolygonPoints)
            {
                avgDistance += Vector3.Distance(point, center);
            }
            avgDistance /= supportPolygonPoints.Count;

            float variance = 0f;
            foreach (Vector3 point in supportPolygonPoints)
            {
                float dist = Vector3.Distance(point, center);
                variance += Mathf.Pow(dist - avgDistance, 2);
            }
            variance /= supportPolygonPoints.Count;

            // Lower variance = better distribution
            return Mathf.Clamp01(1f - (variance / (avgDistance * avgDistance)));
        }

        /// <summary>
        /// Analyzes oscillation in the tower
        /// </summary>
        private float AnalyzeOscillationStability()
        {
            if (centerOfMassHistory.Count < HISTORY_SIZE)
            {
                return 100f; // Not enough data
            }

            // Calculate oscillation magnitude
            Vector3 avgPosition = Vector3.zero;
            foreach (Vector3 pos in centerOfMassHistory)
            {
                avgPosition += pos;
            }
            avgPosition /= centerOfMassHistory.Count;

            float totalDeviation = 0f;
            foreach (Vector3 pos in centerOfMassHistory)
            {
                totalDeviation += Vector3.Distance(pos, avgPosition);
            }
            oscillationMagnitude = totalDeviation / centerOfMassHistory.Count;

            // Apply damping
            oscillationMagnitude *= oscillationDampingFactor;

            // Convert to stability score
            float maxAllowedOscillation = 0.5f;
            return Mathf.Clamp01(1f - (oscillationMagnitude / maxAllowedOscillation)) * 100f;
        }

        private void UpdateCenterOfMassHistory(Vector3 currentCOM)
        {
            centerOfMassHistory.Enqueue(currentCOM);

            while (centerOfMassHistory.Count > HISTORY_SIZE)
            {
                centerOfMassHistory.Dequeue();
            }
        }

        /// <summary>
        /// Analyzes tilt angle of the tower
        /// </summary>
        private float AnalyzeTiltStability()
        {
            if (analyzedPieces.Count == 0) return 100f;

            // Calculate average piece orientation
            Vector3 avgUp = Vector3.zero;
            int count = 0;

            foreach (GameObject piece in analyzedPieces)
            {
                if (piece != null)
                {
                    avgUp += piece.transform.up;
                    count++;
                }
            }

            if (count == 0) return 100f;

            avgUp /= count;

            // Calculate angle from vertical
            float angle = Vector3.Angle(avgUp, Vector3.up);

            // Convert to stability score
            float maxAllowedTilt = 30f;
            return Mathf.Clamp01(1f - (angle / maxAllowedTilt)) * 100f;
        }

        /// <summary>
        /// Gets the current stability score
        /// </summary>
        public float GetStability()
        {
            return currentStability;
        }

        /// <summary>
        /// Gets detailed stability information
        /// </summary>
        public StabilityInfo GetStabilityInfo()
        {
            return new StabilityInfo
            {
                overallStability = currentStability,
                centerOfMass = towerCenterOfMass,
                supportPolygon = new List<Vector3>(supportPolygonPoints),
                oscillationMagnitude = oscillationMagnitude,
                isStable = currentStability > Constants.STABILITY_CRITICAL_THRESHOLD
            };
        }

        private void OnDrawGizmos()
        {
            if (!showDebugVisualization) return;

            // Draw center of mass
            Gizmos.color = currentStability > Constants.STABILITY_WARNING_THRESHOLD ?
                          centerOfMassColor : unstableColor;
            Gizmos.DrawWireSphere(towerCenterOfMass, 0.2f);

            // Draw projection line
            Vector3 projectedCOM = new Vector3(towerCenterOfMass.x, 0, towerCenterOfMass.z);
            Gizmos.DrawLine(towerCenterOfMass, projectedCOM);
            Gizmos.DrawWireCube(projectedCOM, Vector3.one * 0.1f);

            // Draw support polygon
            if (supportPolygonPoints.Count >= 3)
            {
                Gizmos.color = currentStability > Constants.STABILITY_CRITICAL_THRESHOLD ?
                              supportPolygonColor : unstableColor;

                for (int i = 0; i < supportPolygonPoints.Count; i++)
                {
                    Vector3 p1 = supportPolygonPoints[i];
                    Vector3 p2 = supportPolygonPoints[(i + 1) % supportPolygonPoints.Count];

                    p1.y = 0.01f;
                    p2.y = 0.01f;

                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawWireSphere(p1, 0.05f);
                }
            }
        }
    }

    /// <summary>
    /// Data structure for stability information
    /// </summary>
    [System.Serializable]
    public struct StabilityInfo
    {
        public float overallStability;
        public Vector3 centerOfMass;
        public List<Vector3> supportPolygon;
        public float oscillationMagnitude;
        public bool isStable;
    }
}