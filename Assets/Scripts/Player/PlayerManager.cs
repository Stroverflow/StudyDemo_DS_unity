using System;
using System.Collections;
using System.Collections.Generic;
using SG;
using UnityEngine;

namespace SG
{
    /**
     * 角色管理器
     * 管理所有和角色有关的功能
     */
    public class PlayerManager : MonoBehaviour
    {
        private InputHandler _inputHandler;
        private CameraHandler _cameraHandler;
        private Animator anim;
        private PlayerLocomotion _playerLocomotion;
        
        public bool isInteracting;
        
        [Header("Player Flags")]
        public bool isSprinting;
        public bool isInAir;
        public bool isGrounded;
        public bool canDoCombo;

        private void Awake()
        {
            _cameraHandler = FindObjectOfType<CameraHandler>();
            
        }
        
        private void Start()
        {
            _inputHandler = GetComponent<InputHandler>();
            anim = GetComponentInChildren<Animator>();
            _playerLocomotion = GetComponent<PlayerLocomotion>();
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            
            isInteracting = anim.GetBool("isInteracting");
            canDoCombo = anim.GetBool("canDoCombo");
            
            _inputHandler.TickInput(delta);
            _playerLocomotion.HandleMovement(delta);
            _playerLocomotion.HandleRollingAndSprinting(delta);
            _playerLocomotion.HandleFalling(delta, _playerLocomotion.moveDirection);
        }
        
        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;

            if (_cameraHandler != null)
            {
                _cameraHandler.FollowTarget(delta);
                _cameraHandler.HandleCameraRotation(delta, _inputHandler.mouseX, _inputHandler.mouseY);
            }
        }

        private void LateUpdate()
        {
            _inputHandler.rollFlag = false;
            _inputHandler.sprintFlag = false;
            _inputHandler.rb_Input = false;
            _inputHandler.rt_Input = false;

            if (isInAir)
            {
                _playerLocomotion.inAirTimer = _playerLocomotion.inAirTimer + Time.deltaTime;
            }
        }
    }
}

