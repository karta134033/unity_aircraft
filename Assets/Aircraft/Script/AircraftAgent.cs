using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aircraft {
    public class AircraftAgent : MonoBehaviour {
        public float thrust = 10000f;
        public float pitchSpeed = 100f;
        public float yawSpeed = 100f;
        public float rollSpeed = 100f;
        public float boostMultiplier = 2f;
        public int NextCheckpointIndex { get; set; }
        private AircraftArea area;
        new private Rigidbody rigidbody;
        private TrailRenderer trail;

        private float pitchChange = 0f;
        private float smoothPitchChange = 0f;
        private float maxPitchAngle = 45f;
        private float yawChange = 0f;
        private float smoothYawChange = 0f;
        private float rollChange = 0f;
        private float smoothRollChange = 0f;
        private float maxRollAngle = 45f;
        private bool boost;
    }
}