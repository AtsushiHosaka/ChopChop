using UnityEngine;

namespace ChopChop.Presentation
{
    [AddComponentMenu("ChopChop/Presentation/Waypoint Rail Mover")]
    public sealed class WaypointRailMover : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform[] waypoints = new Transform[0];
        [SerializeField] private float moveSeconds = 0.75f;

        private int currentIndex;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private float moveStartedAt;
        private bool moving;

        public int CurrentIndex => currentIndex;

        public void Configure(Transform movementTarget, Transform[] railWaypoints, float seconds)
        {
            target = movementTarget;
            waypoints = railWaypoints ?? new Transform[0];
            moveSeconds = seconds;
        }

        public void MoveTo(int index)
        {
            if (target == null || waypoints == null || index < 0 || index >= waypoints.Length || waypoints[index] == null)
            {
                return;
            }

            currentIndex = index;
            startPosition = target.position;
            startRotation = target.rotation;
            moveStartedAt = Time.time;
            moving = true;
        }

        private void Reset()
        {
            target = transform;
        }

        private void Update()
        {
            if (!moving || target == null)
            {
                return;
            }

            var waypoint = waypoints[currentIndex];
            if (waypoint == null)
            {
                moving = false;
                return;
            }

            var t = moveSeconds <= 0f ? 1f : Mathf.Clamp01((Time.time - moveStartedAt) / moveSeconds);
            target.position = Vector3.Lerp(startPosition, waypoint.position, Smooth(t));
            target.rotation = Quaternion.Slerp(startRotation, waypoint.rotation, Smooth(t));

            if (t >= 1f)
            {
                moving = false;
            }
        }

        private static float Smooth(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
