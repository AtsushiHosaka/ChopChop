using System;
using ChopChop.Presentation;
using UnityEngine;

namespace ChopChop.Levels.Sukiyaki
{
    [ExecuteAlways]
    [AddComponentMenu("ChopChop/Levels/Sukiyaki Stage Layout")]
    public sealed class SukiyakiStageLayout : MonoBehaviour
    {
        [SerializeField] private bool applyInEditMode = true;
        [SerializeField] private Transform hashiRoot;
        [SerializeField] private WaypointRailMover railMover;
        [SerializeField] private float railMoveSeconds = 0.75f;

        [SerializeField] private StagePoint calibration = new StagePoint(
            "Calibration",
            new Vector3(-2.15f, 1f, -0.85f),
            Vector3.zero);

        [SerializeField] private StagePoint egg = new StagePoint(
            "Egg",
            new Vector3(-1.85f, 0.015f, 0.02f),
            new Vector3(0f, 0.905f, -0.36f));

        [SerializeField] private StagePoint rawMeat = new StagePoint(
            "Raw Meat",
            new Vector3(-0.75f, 0.005f, -0.52f),
            new Vector3(0f, 1.14f, -0.43f));

        [SerializeField] private StagePoint broth = new StagePoint(
            "Broth",
            new Vector3(0.72f, 0.005f, -0.28f),
            new Vector3(0f, 1.17f, -0.4f));

        [SerializeField] private StagePoint flip = new StagePoint(
            "Flip",
            new Vector3(0.15f, 0.015f, 0.34f),
            new Vector3(0f, 1.005f, -0.46f));

        [SerializeField] private StagePoint mouth = new StagePoint(
            "Mouth",
            new Vector3(1.75f, 0.22f, -0.08f),
            new Vector3(0f, 0.78f, -0.47f));

        public StagePoint Calibration => calibration;
        public StagePoint Egg => egg;
        public StagePoint RawMeat => rawMeat;
        public StagePoint Broth => broth;
        public StagePoint Flip => flip;
        public StagePoint Mouth => mouth;

        [ContextMenu("Apply Layout")]
        public void ApplyLayout()
        {
            calibration.Apply();
            egg.Apply();
            rawMeat.Apply();
            broth.Apply();
            flip.Apply();
            mouth.Apply();
            ConfigureRail();
        }

        [ContextMenu("Capture Positions From Scene")]
        public void CapturePositionsFromScene()
        {
            calibration.Capture();
            egg.Capture();
            rawMeat.Capture();
            broth.Capture();
            flip.Capture();
            mouth.Capture();
        }

        public Transform[] GetOrderedWaypoints()
        {
            return new[]
            {
                calibration.Waypoint,
                egg.Waypoint,
                rawMeat.Waypoint,
                broth.Waypoint,
                flip.Waypoint,
                mouth.Waypoint
            };
        }

        private void Reset()
        {
            hashiRoot = GameObject.Find("Hashi")?.transform;
            railMover = hashiRoot != null ? hashiRoot.GetComponent<WaypointRailMover>() : FindAnyObjectByType<WaypointRailMover>();
        }

        private void OnValidate()
        {
            if (applyInEditMode && !Application.isPlaying)
            {
                ApplyLayout();
            }
        }

        private void ConfigureRail()
        {
            if (railMover == null || hashiRoot == null)
            {
                return;
            }

            railMover.Configure(hashiRoot, GetOrderedWaypoints(), railMoveSeconds);
        }

        [Serializable]
        public sealed class StagePoint
        {
            [SerializeField] private string label;
            [Tooltip("Scene anchor for the station. Move this by editing Position.")]
            [SerializeField] private Transform stationRoot;
            [Tooltip("Waypoint used by WaypointRailMover for the corresponding gameplay phase.")]
            [SerializeField] private Transform waypoint;
            [Tooltip("World position of the station anchor. If Station Root is empty, this controls the waypoint directly.")]
            [SerializeField] private Vector3 position;
            [Tooltip("World rotation of the station anchor.")]
            [SerializeField] private Vector3 eulerAngles;
            [Tooltip("Waypoint position relative to Position.")]
            [SerializeField] private Vector3 waypointOffset;
            [Tooltip("World rotation of the waypoint.")]
            [SerializeField] private Vector3 waypointEulerAngles = new Vector3(65f, 0f, 0f);

            public string Label => label;
            public Transform StationRoot => stationRoot;
            public Transform Waypoint => waypoint;
            public Vector3 Position => position;
            public Vector3 WaypointPosition => position + waypointOffset;

            public StagePoint(string label, Vector3 position, Vector3 waypointOffset)
            {
                this.label = label;
                this.position = position;
                this.waypointOffset = waypointOffset;
            }

            public void Configure(Transform station, Transform railWaypoint)
            {
                stationRoot = station;
                waypoint = railWaypoint;
            }

            public void Apply()
            {
                if (stationRoot != null)
                {
                    stationRoot.SetPositionAndRotation(position, Quaternion.Euler(eulerAngles));
                }

                if (waypoint != null)
                {
                    var waypointPosition = stationRoot != null ? position + waypointOffset : position;
                    waypoint.SetPositionAndRotation(waypointPosition, Quaternion.Euler(waypointEulerAngles));
                }
            }

            public void Capture()
            {
                if (stationRoot != null)
                {
                    position = stationRoot.position;
                    eulerAngles = stationRoot.eulerAngles;
                }
                else if (waypoint != null)
                {
                    position = waypoint.position;
                }

                if (waypoint != null)
                {
                    waypointOffset = stationRoot != null ? waypoint.position - position : Vector3.zero;
                    waypointEulerAngles = waypoint.eulerAngles;
                }
            }
        }
    }
}
