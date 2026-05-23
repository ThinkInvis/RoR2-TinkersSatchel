using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NodeGraph = RoR2.Navigation.NodeGraph;

namespace ThinkInvisible.TinkersSatchel {
    public static class MathUtil {
        /// <summary>
        /// Wraps a float within the bounds of two other floats.
        /// </summary>
        /// <param name="x">The number to perform a wrap operation on.</param>
        /// <param name="min">The lower bound of the wrap operation.</param>
        /// <param name="max">The upper bound of the wrap operation.</param>
        /// <returns>The result of the wrap operation of x within min, max.</returns>
        public static float Wrap(float x, float min, float max) {
            if(x < min)
                return max - (min - x) % (max - min);
            else
                return min + (x - min) % (max - min);
        }
        public static float SteepSigmoid01(float x, float b) {
            return 0.5f - (float)Math.Tanh(2 * b * (x - 0.5f)) / (2f * (float)Math.Tanh(-b));
        }

        internal static Quaternion ApplyRandomSpread(Quaternion targetRotation, float coneHalfAngleDegr) {
            var phi = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            var z = UnityEngine.Random.Range(Mathf.Cos(coneHalfAngleDegr * Mathf.PI / 180f), 1f);
            var zf = Mathf.Sqrt(1f - z * z);
            var rDir = new Vector3(zf * Mathf.Cos(phi), zf * Mathf.Sin(phi), z);
            return targetRotation * Quaternion.LookRotation(rDir);
        }

        /// <summary>
        /// Calculates the initial velocity and final time required for a jump-pad-like trajectory between two points.
        /// </summary>
        /// <param name="source">The starting point of the trajectory.</param>
        /// <param name="target">The endpoint of the trajectory.</param>
        /// <param name="extraPeakHeight">Extra height to add above the apex of the lowest possible trajectory.</param>
        /// <returns>vInitial: initial velocity of the trajectory. tFinal: time required to reach target from source.</returns>
        public static (Vector3 vInitial, float tFinal) CalculateVelocityForFinalPosition(Vector3 source, Vector3 target, float extraPeakHeight) {
            var deltaPos = target - source;
            var yF = deltaPos.y;
            var yPeak = Mathf.Max(Mathf.Max(yF, 0) + extraPeakHeight, yF, 0);
            //everything will be absolutely ruined if gravity goes in any direction other than -y. them's the breaks.
            var g = -Physics.gravity.y;
            //calculate initial vertical velocity
            float vY0 = Mathf.Sqrt(2f * g * yPeak);
            //calculate total travel time from vertical velocity
            float tF = Mathf.Sqrt(2) / g * (Mathf.Sqrt(g * (yPeak - yF)) + Mathf.Sqrt(g * yPeak));
            //use total travel time to calculate other velocity components
            var vX0 = deltaPos.x / tF;
            var vZ0 = deltaPos.z / tF;
            return (new Vector3(vX0, vY0, vZ0), tF);
        }

        /// <summary>
        /// Performs a spherecast over a parabolic trajectory.
        /// </summary>
        /// <param name="hit">If a hit occurred: the resultant RaycastHit. Otherwise: default(RaycastHit).</param>
        /// <param name="source">The starting point of the trajectory.</param>
        /// <param name="vInitial">The starting velocity of the trajectory.</param>
        /// <param name="tFinal">The total travel time of the trajectory.</param>
        /// <param name="radius">As with straight-line UnityEngine.Physics.Spherecast.</param>
        /// <param name="resolution">How many individual spherecasts to perform over the trajectory path.</param>
        /// <param name="layerMask">As with straight-line UnityEngine.Physics.Spherecast.</param>
        /// <param name="qTI">As with straight-line UnityEngine.Physics.Spherecast.</param>
        /// <returns>True iff a spherecast hit occurred.</returns>
        public static bool TrajectorySphereCast(out RaycastHit hit, Vector3 source, Vector3 vInitial, float tFinal, float radius, int resolution, int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction qTI = QueryTriggerInteraction.UseGlobal) {
            Vector3 p0, p1;
            p1 = source;
            for(var i = 0; i < resolution; i++) {
                p0 = p1;
                p1 = Trajectory.CalculatePositionAtTime(source, vInitial, ((float)i / (float)resolution + 1f) * tFinal);
                var del = (p1 - p0);
                var didHit = Physics.SphereCast(new Ray(p0, del.normalized), radius, out hit, del.magnitude, layerMask, qTI);
                if(didHit) return true;
            }
            hit = default;
            return false;
        }

        /// <summary>
        /// Collects a specified number of launch velocities that will reach (without hitting anything else) the nearest free navnodes outside a minimum range.
        /// </summary>
        /// <param name="graph">The nodegraph to find nodes from.</param>
        /// <param name="desiredCount">The ideal number of nodes to find.</param>
        /// <param name="minRange">The minimum range to find nodes within.</param>
        /// <param name="maxRange">The maximum range to find nodes within.</param>
        /// <param name="source">The starting point of all trajectories, and the point to search for nodes around.</param>
        /// <param name="extraPeakHeight">See CalculateVelocityForFinalPosition.</param>
        /// <param name="radius">See TrajectorySphereCast.</param>
        /// <param name="maxDeviation">Distance any TrajectorySphereCast hit is allowed to be from its target node to count as a valid result.</param>
        /// <param name="trajectoryResolution">See TrajectorySphereCast.</param>
        /// <param name="layerMask">See TrajectorySphereCast.</param>
        /// <param name="qTI">See TrajectorySphereCast.</param>
        /// <param name="hullMask">Passed through to NodeGraph.FindNodesInRange.</param>
        /// <returns>A list of between 0 and desiredCount launch velocities. Less results will be returned if not enough clear paths to open nodes with the given parameters can be found.</returns>
        public static List<Vector3> CollectNearestNodeLaunchVelocities(
            NodeGraph graph, int desiredCount, float minRange, float maxRange,
            Vector3 source, float extraPeakHeight, float radius, float maxDeviation, int trajectoryResolution,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction qTI = QueryTriggerInteraction.UseGlobal, HullMask hullMask = HullMask.Human) {
            var nodeLocs = graph.FindNodesInRange(source, minRange, maxRange, hullMask)
                .Select(x => { graph.GetNodePosition(x, out Vector3 xloc); return xloc; })
                .OrderBy(x => (x - source).sqrMagnitude);

            List<Vector3> retv = new();

            var mDevSq = maxDeviation * maxDeviation;

            foreach(var loc in nodeLocs) {
                var (vInitial, tFinal) = CalculateVelocityForFinalPosition(source, loc, extraPeakHeight);
                var didHit = TrajectorySphereCast(out RaycastHit hit,
                    source, vInitial, tFinal,
                    radius, trajectoryResolution, layerMask, qTI);
                if(didHit && (hit.point - loc).sqrMagnitude <= mDevSq)
                    retv.Add(vInitial);
                if(retv.Count >= desiredCount)
                    break;
            }

            return retv;
        }
    }
}
