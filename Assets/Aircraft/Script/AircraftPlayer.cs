using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aircraft {
    public class AircraftPlayer : AircraftAgent {
        [Header("Input Bindings")]
        public InputAction pitchInput;
        public InputAction yawInput;
        public InputAction boostInput;
        public InputAction pauseInput;

        public override void Initialize() {
            base.Initialize();

            pitchInput.Enable();
            yawInput.Enable();
            boostInput.Enable();
            pauseInput.Enable();
        }

        public override void Heuristic(float[] actionsOut) {   // 將使用者的輸入傳入到OnActionReceived
            float pitchValue = Mathf.Round(pitchInput.ReadValue<float>());  // pitch 1 == 上, 0 == none, -1 == 下
            float yawValue = Mathf.Round(yawInput.ReadValue<float>());  // yaw 1 == 右, 0 == none, -1 == left
            float boostValue = Mathf.Round(boostInput.ReadValue<float>());  // boost 1 == boost, 0 == none,
            if (pitchValue == -1f) 
                pitchValue = 2f;
            if (yawValue == -1f) 
                yawValue = 2f;
            
            actionsOut[0] = pitchValue;
            actionsOut[1] = yawValue;
            actionsOut[2] = boostValue;
        }

        private void OnDestory() {  // destory時清理掉所有Input
            pitchInput.Disable();
            yawInput.Disable();
            boostInput.Disable();
            pauseInput.Disable();
        }
    }
}
