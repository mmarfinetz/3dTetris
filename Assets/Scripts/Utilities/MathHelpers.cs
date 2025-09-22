using UnityEngine;
using System.Collections.Generic;

namespace TetrisJenga.Utilities
{
    /// <summary>
    /// Mathematical helper functions
    /// </summary>
    public static class MathHelpers
    {
        /// <summary>
        /// Remaps a value from one range to another
        /// </summary>
        public static float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Smoothstep interpolation
        /// </summary>
        public static float SmoothStep(float edge0, float edge1, float x)
        {
            x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return x * x * (3f - 2f * x);
        }

        /// <summary>
        /// Smoother step interpolation
        /// </summary>
        public static float SmootherStep(float edge0, float edge1, float x)
        {
            x = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return x * x * x * (x * (x * 6f - 15f) + 10f);
        }

        /// <summary>
        /// Calculates the signed angle between two vectors on a plane
        /// </summary>
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
            float angle = Vector3.Angle(from, to);
            float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(from, to)));
            return angle * sign;
        }

        /// <summary>
        /// Projects a point onto a plane
        /// </summary>
        public static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeNormal, Vector3 planePoint)
        {
            float distance = -Vector3.Dot(planeNormal.normalized, point - planePoint);
            return point + planeNormal.normalized * distance;
        }

        /// <summary>
        /// Calculates the closest point on a line segment
        /// </summary>
        public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;
            lineDirection.Normalize();

            float projectLength = Mathf.Clamp(Vector3.Dot(point - lineStart, lineDirection), 0f, lineLength);
            return lineStart + lineDirection * projectLength;
        }

        /// <summary>
        /// Calculates the distance from a point to a line segment
        /// </summary>
        public static float DistanceToLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 closestPoint = ClosestPointOnLineSegment(point, lineStart, lineEnd);
            return Vector3.Distance(point, closestPoint);
        }

        /// <summary>
        /// Checks if a point is inside a triangle
        /// </summary>
        public static bool IsPointInTriangle(Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = point - a;

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        /// <summary>
        /// Calculates the area of a polygon
        /// </summary>
        public static float CalculatePolygonArea(List<Vector3> vertices)
        {
            float area = 0f;
            int n = vertices.Count;

            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += vertices[i].x * vertices[j].z;
                area -= vertices[j].x * vertices[i].z;
            }

            return Mathf.Abs(area) * 0.5f;
        }

        /// <summary>
        /// Calculates the centroid of a polygon
        /// </summary>
        public static Vector3 CalculatePolygonCentroid(List<Vector3> vertices)
        {
            Vector3 centroid = Vector3.zero;
            float signedArea = 0f;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v0 = vertices[i];
                Vector3 v1 = vertices[(i + 1) % vertices.Count];

                float a = v0.x * v1.z - v1.x * v0.z;
                signedArea += a;

                centroid.x += (v0.x + v1.x) * a;
                centroid.z += (v0.z + v1.z) * a;
            }

            signedArea *= 0.5f;
            centroid.x /= (6f * signedArea);
            centroid.z /= (6f * signedArea);

            return centroid;
        }

        /// <summary>
        /// Performs a raycast against a plane
        /// </summary>
        public static bool RayPlaneIntersection(Ray ray, Vector3 planeNormal, Vector3 planePoint, out float distance)
        {
            float denominator = Vector3.Dot(ray.direction, planeNormal);

            if (Mathf.Abs(denominator) < 0.0001f)
            {
                distance = 0f;
                return false;
            }

            distance = Vector3.Dot(planePoint - ray.origin, planeNormal) / denominator;
            return distance >= 0f;
        }

        /// <summary>
        /// Calculates a smooth damped motion
        /// </summary>
        public static float SmoothDamp(float current, float target, ref float velocity, float smoothTime, float maxSpeed = Mathf.Infinity)
        {
            smoothTime = Mathf.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;
            float x = omega * Time.deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);

            float change = current - target;
            float originalTo = target;

            float maxChange = maxSpeed * smoothTime;
            change = Mathf.Clamp(change, -maxChange, maxChange);
            target = current - change;

            float temp = (velocity + omega * change) * Time.deltaTime;
            velocity = (velocity - omega * temp) * exp;
            float output = target + (change + temp) * exp;

            if (originalTo - current > 0.0f == output > originalTo)
            {
                output = originalTo;
                velocity = (output - originalTo) / Time.deltaTime;
            }

            return output;
        }

        /// <summary>
        /// Generates a random point inside a unit circle
        /// </summary>
        public static Vector2 RandomInsideUnitCircle()
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Mathf.Sqrt(Random.Range(0f, 1f));
            return new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        }

        /// <summary>
        /// Calculates the bounding box of multiple bounds
        /// </summary>
        public static Bounds CalculateBounds(List<Bounds> boundsList)
        {
            if (boundsList == null || boundsList.Count == 0)
                return new Bounds(Vector3.zero, Vector3.zero);

            Bounds combined = boundsList[0];
            for (int i = 1; i < boundsList.Count; i++)
            {
                combined.Encapsulate(boundsList[i]);
            }

            return combined;
        }

        /// <summary>
        /// Easing function for animations
        /// </summary>
        public static class Easing
        {
            public static float EaseInQuad(float t)
            {
                return t * t;
            }

            public static float EaseOutQuad(float t)
            {
                return t * (2f - t);
            }

            public static float EaseInOutQuad(float t)
            {
                return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
            }

            public static float EaseInCubic(float t)
            {
                return t * t * t;
            }

            public static float EaseOutCubic(float t)
            {
                return (--t) * t * t + 1f;
            }

            public static float EaseInOutCubic(float t)
            {
                return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
            }

            public static float EaseInElastic(float t)
            {
                if (t == 0f || t == 1f) return t;
                float p = 0.3f;
                float s = p / 4f;
                return -(Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p));
            }

            public static float EaseOutElastic(float t)
            {
                if (t == 0f || t == 1f) return t;
                float p = 0.3f;
                float s = p / 4f;
                return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
            }

            public static float EaseInOutElastic(float t)
            {
                if (t == 0f || t == 1f) return t;
                float p = 0.3f * 1.5f;
                float s = p / 4f;

                if (t < 0.5f)
                {
                    return -0.5f * Mathf.Pow(2f, 10f * (t -= 0.5f)) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p);
                }
                return Mathf.Pow(2f, -10f * (t -= 0.5f)) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) * 0.5f + 1f;
            }

            public static float EaseInBounce(float t)
            {
                return 1f - EaseOutBounce(1f - t);
            }

            public static float EaseOutBounce(float t)
            {
                if (t < (1f / 2.75f))
                {
                    return 7.5625f * t * t;
                }
                else if (t < (2f / 2.75f))
                {
                    return 7.5625f * (t -= (1.5f / 2.75f)) * t + 0.75f;
                }
                else if (t < (2.5f / 2.75f))
                {
                    return 7.5625f * (t -= (2.25f / 2.75f)) * t + 0.9375f;
                }
                else
                {
                    return 7.5625f * (t -= (2.625f / 2.75f)) * t + 0.984375f;
                }
            }

            public static float EaseInOutBounce(float t)
            {
                return t < 0.5f ? EaseInBounce(t * 2f) * 0.5f : EaseOutBounce(t * 2f - 1f) * 0.5f + 0.5f;
            }
        }
    }
}