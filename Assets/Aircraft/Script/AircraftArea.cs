using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Aircraft {
    public class AircraftArea : MonoBehaviour {
        public CinemachineSmoothPath racePath;
        public GameObject checkpointPrefab;
        public GameObject finishCheckpointPrefab;
        public bool trainingMode;
        public List<AircraftAgent> AircraftAgents { get; private set; }
        public List<GameObject> Checkpoints { get; private set; }

        /// <sumary>
        /// 這個script被使用時會呼叫此函式
        /// </summary>
        private void Awake() {
            if (AircraftAgents == null) FindAircraftAgents();
        }

        private void Start() {
            if (Checkpoints == null) CreateCheckpoints();
        }
        private void FindAircraftAgents() {
            AircraftAgents = transform.GetComponentsInChildren<AircraftAgent>().ToList();
        }

        private void CreateCheckpoints() {
            Checkpoints = new List<GameObject>();
            int numCheckpoint = (int)racePath.MaxUnit(CinemachinePathBase.PositionUnits.PathUnits);
            for (int i = 0; i < numCheckpoint; i++) {
                GameObject checkpoint;
                if (i == numCheckpoint - 1) checkpoint = Instantiate<GameObject>(finishCheckpointPrefab);
                else checkpoint = Instantiate<GameObject>(checkpointPrefab);
                checkpoint.transform.SetParent(racePath.transform);  // 將checkpoint轉向與設定位置
                checkpoint.transform.localPosition = racePath.m_Waypoints[i].position;
                checkpoint.transform.rotation = racePath.EvaluateOrientationAtUnit(i, CinemachinePathBase.PositionUnits.PathUnits);

                Checkpoints.Add(checkpoint);
            }
        }

        /// <sumary>
        /// 重設agent到前一個checkpoint
        /// randomize為true則會選擇任何checkpoint當重生點
        /// </summary>
        public void ResetAgentPosition(AircraftAgent agent, bool randomize = false) {
            if (AircraftAgents == null) FindAircraftAgents();
            if (Checkpoints == null) CreateCheckpoints();

            if (randomize) {
                agent.NextCheckpointIndex = Random.Range(0, Checkpoints.Count);
            }
            int previousCheckpointIndex = agent.NextCheckpointIndex - 1;
            if (previousCheckpointIndex == -1) previousCheckpointIndex = Checkpoints.Count - 1;

            float startPosition = racePath.FromPathNativeUnits(previousCheckpointIndex, CinemachinePathBase.PositionUnits.PathUnits);
            Vector3 basePosition = racePath.EvaluatePosition(startPosition);

            Quaternion orientation = racePath.EvaluateOrientation(startPosition);
            Vector3 positionOffset = Vector3.right * (AircraftAgents.IndexOf(agent) - AircraftAgents.Count / 2f) * Random.Range(9f, 10f);

            agent.transform.position = basePosition + orientation * positionOffset;
            agent.transform.rotation = orientation;
        }
    }

}