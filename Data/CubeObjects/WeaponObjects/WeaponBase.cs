using Godot;
using System;

namespace Stellacrum.Data.CubeObjects.WeaponObjects
{
    public class WeaponBase
    {
        //public static Vector3 Interception(Vector3 selfPos, Vector3 targetPos, Vector3 selfVel, Vector3 targetVel)
        //{
        //    // Calculate relative velocity
        //    Vector3 relativeVelocity = targetVel - selfVel;
        //
        //    // Check if the target is moving away from the pursuer
        //    float relativeSpeedSquared = relativeVelocity.Dot(relativeVelocity);
        //    if (relativeSpeedSquared < float.Epsilon)
        //    {
        //        // Target is not moving relative to the pursuer, no interception point
        //        return targetPos;
        //    }
        //
        //    // Calculate time of intersection
        //    float timeToIntersection = (targetPos - selfPos).Dot(relativeVelocity) / relativeSpeedSquared;
        //
        //    // Calculate interception point
        //    Vector3 interceptionPoint = targetPos + targetVel * timeToIntersection;
        //
        //    return interceptionPoint;
        //}

        public static Vector3? CalculateInterceptionPoint(Vector3 selfPosition, Vector3 selfVelocity, Vector3 targetPosition, Vector3 targetVelocity, float projectileSpeed)
        {
            Vector3 relativeVelocity = targetVelocity - selfVelocity;

            // Calculate time of interception
            try
            {
                float t = CalculateTimeOfInterception(selfPosition, targetPosition, relativeVelocity, projectileSpeed);
                // Calculate interception point
                Vector3 interceptionPoint = targetPosition + relativeVelocity * t;

                return interceptionPoint;
            }
            catch
            {
                return null;
            }
        }

        static float CalculateTimeOfInterception(Vector3 selfPosition, Vector3 targetPosition, Vector3 relativeVelocity, float projectileSpeed)
        {
            // Calculate quadratic equation coefficients
            float a = relativeVelocity.Dot(relativeVelocity) - projectileSpeed * projectileSpeed;
            float b = 2 * relativeVelocity.Dot(targetPosition - selfPosition);
            float c = (targetPosition - selfPosition).Dot(targetPosition - selfPosition);

            // Solve quadratic equation for time
            float discriminant = b * b - 4 * a * c;

            if (discriminant < 0)
            {
                // No real solutions, interception not possible
                throw new InvalidOperationException("Interception not possible.");
            }

            float t1 = (-b + MathF.Sqrt(discriminant)) / (2 * a);
            float t2 = (-b - MathF.Sqrt(discriminant)) / (2 * a);

            // Return the positive real solution, if any
            if (t1 > 0 && t2 > 0)
                return MathF.Min(t1, t2);

            if (t1 > 0)
            {
                return t1;
            }
            else if (t2 > 0)
            {
                return t2;
            }
            else
            {
                // No positive real solutions, interception not possible
                throw new InvalidOperationException("Interception not possible.");
            }
        }
    }
}
