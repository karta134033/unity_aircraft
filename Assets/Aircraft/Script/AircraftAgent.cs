using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

namespace Aircraft {
    public class AircraftAgent : Agent {
        public float thrust = 100000f;
        public float pitchSpeed = 100f;
        public float yawSpeed = 100f;
        public float rollSpeed = 100f;
        public float boostMultiplier = 2f;
        public int NextCheckpointIndex { get; set; }

        private AircraftArea area;
        new private Rigidbody rigidbody;
        private TrailRenderer trail;

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
        }

        public override void OnActionReceived(float[] vectorAction) {
            pitchChange = vectorAction[0];  // 上
            if (pitchChange == 2) pitchChange = -1f;  // 下    因為mlagent的關係，所以值會在正數 (0~正數)
            yawChange = vectorAction[1];  // 右
            if (yawChange == 2) yawChange = -1f;  // 左

            boost = vectorAction[2] == 1;
            if (boost && !trail.emitting) trail.Clear();
            trail.emitting = boost;

            ProcessMovement();
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
    }
}