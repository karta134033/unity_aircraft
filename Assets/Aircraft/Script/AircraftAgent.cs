using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

namespace Aircraft {
    public class AircraftAgent : Agent {
        public float thrust = 100000f;
        public float pitchSpeed = 100f;
        public float yawSpeed = 100f;
        public float rollSpeed = 100f;
        public float boostMultiplier = 2f;
        public int NextCheckpointIndex { get; set; }

        public GameObject meshObject;  // 飛機會在爆炸時消失
        public GameObject explosionEffect;  // 爆炸時的特效
        public int stepTimeout = 300;

        // 需要持續監控的原件
        private AircraftArea area;
        new private Rigidbody rigidbody;
        private TrailRenderer trail;

        private float nextStepTimeout;

        private bool frozen = false;  // 暫停與爆炸時會凍結住

        // 控制
        private float pitchChange = 0f;
        private float smoothPitchChange = 0f;
        private float maxPitchAngle = 45f;
        private float yawChange = 0f;
        private float smoothYawChange = 0f;
        private float rollChange = 0f;
        private float smoothRollChange = 0f;
        private float maxRollAngle = 45f;
        private bool boost;

        public override void Initialize() {
            area = GetComponentInParent<AircraftArea>();
            rigidbody = GetComponent<Rigidbody>();
            trail = GetComponent<TrailRenderer>();

            MaxStep = area.trainingMode ? 5000 : 0;
        }

        public override void OnEpisodeBegin() {  // 每當新的Training Episode開始時會觸發
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            trail.emitting = false;
            area.ResetAgentPosition(agent: this, randomize: area.trainingMode);  // 在訓練時期會隨機分配到某個Checkpoint

            if (area.trainingMode) nextStepTimeout = StepCount + stepTimeout;
        }

        public override void OnActionReceived(float[] vectorAction) {
            if (frozen) return;

            pitchChange = vectorAction[0];  // 上
            if (pitchChange == 2) pitchChange = -1f;  // 下    因為mlagent的關係，所以值會在正數 (0~正數)
            yawChange = vectorAction[1];  // 右
            if (yawChange == 2) yawChange = -1f;  // 左

            boost = vectorAction[2] == 1;
            if (boost && !trail.emitting) trail.Clear();
            trail.emitting = boost;

            ProcessMovement();

            if (area.trainingMode) {
                AddReward(-1f / MaxStep);  // 每走一步會有負的reward

                if (StepCount > nextStepTimeout) {  // 確保訓練時不會把時間後耗完
                    AddReward(-.5f);
                    EndEpisode();
                }

                Vector3 localCheckpointDir = VectorToNextCheckpoint();
                if (localCheckpointDir.magnitude < Academy.Instance.EnvironmentParameters.GetWithDefault("checkpoint_radius", 0f)) {
                    GotCheckpoint();
                }
            }
        }

        public override void CollectObservations(VectorSensor sensor) {
            sensor.AddObservation(transform.InverseTransformDirection(rigidbody.velocity));  // Vector3
            sensor.AddObservation(VectorToNextCheckpoint());  // Vector3
            Vector3 nextCheckpointForword = area.Checkpoints[NextCheckpointIndex].transform.forward;  // checkpoint 的方向
            sensor.AddObservation(transform.InverseTransformDirection(nextCheckpointForword));  // Vector3
            // 共 9 個 observations
        }

        public override void Heuristic(float[] actionOut) {
            Debug.LogError("正在使用 Heuristic mode");
        }

        public void FreezeAgent() {
            frozen = true;
            rigidbody.Sleep();
            trail.emitting = false;
        }

        public void ThawAgent() {
            frozen = false;
            rigidbody.WakeUp();
        }

        private Vector3 VectorToNextCheckpoint() {
            Vector3 nextCheckpointDir = area.Checkpoints[NextCheckpointIndex].transform.position - transform.position;
            Vector3 localCheckpointDir = transform.InverseTransformDirection(nextCheckpointDir);
            return localCheckpointDir;
        }

        private void GotCheckpoint() {  // 當agent飛過checkpoint時會觸發
            NextCheckpointIndex = (NextCheckpointIndex + 1) % area.Checkpoints.Count;  // area.Checkpoints.Count 確保Index不會超過上限

            if (area.trainingMode) {
                AddReward(.5f);
                nextStepTimeout = StepCount + stepTimeout;  // 更新timeout 時間
            }
        }

        private void ProcessMovement() {
            float boostModifier = boost ? boostMultiplier : 1f;
            rigidbody.AddForce(transform.forward * thrust * boostModifier, ForceMode.Force);  // 向前的動力
            Vector3 currentRotation = transform.rotation.eulerAngles;
            float rollAngle = currentRotation.z > 180f ? currentRotation.z - 360f : currentRotation.z;  // 計算roll angle (-180 ~ 180)
            if (yawChange == 0) {
                rollChange = -rollAngle / maxRollAngle;
            }
            else {
                rollChange = -yawChange;
            }

            smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
            smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);
            smoothRollChange = Mathf.MoveTowards(smoothRollChange, rollChange, 2f * Time.fixedDeltaTime);

            float pitch = currentRotation.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
            float yaw = currentRotation.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;
            float roll = currentRotation.z + smoothRollChange * Time.fixedDeltaTime * rollSpeed;
            if (pitch > 180f) pitch -= 360f;
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);
            if (roll > 180f) roll -= 360f;
            roll = Mathf.Clamp(roll, -maxRollAngle, maxRollAngle);

            transform.rotation = Quaternion.Euler(pitch, yaw, roll);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.transform.CompareTag("checkpoint") && 
                other.gameObject == area.Checkpoints[NextCheckpointIndex]
            ) {
                GotCheckpoint();
            }
        }

        private void OnCollisionEnter(Collision collision) {
            if (!collision.transform.CompareTag("agent")) {  // 當撞到不是agent的物體
                if (area.trainingMode) {
                    AddReward(-1f);
                    EndEpisode();
                }
                else {
                    StartCoroutine(ExplosionReset());
                }

            }
        }

        private IEnumerator ExplosionReset() {  // 重設飛機到最近的checkpoint
            FreezeAgent();
            meshObject.SetActive(false);
            explosionEffect.SetActive(true);
            yield return new WaitForSeconds(2f);

            meshObject.SetActive(true);
            explosionEffect.SetActive(false);

            area.ResetAgentPosition(agent: this);  // 退回到最近的檢查點
            yield return new WaitForSeconds(1f);

            ThawAgent();
        }
    }
}