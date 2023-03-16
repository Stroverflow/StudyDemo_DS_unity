using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SG
{
    /**
     * 玩家移动
     * 处理所有与角色移动相关的功能
     */
    public class PlayerLocomotion : MonoBehaviour
    {
        private Transform cameraObject;
        private InputHandler _inputHandler;
        private PlayerManager _playerManager;
        public Vector3 moveDirection;

        [HideInInspector]
        public Transform myTransform;
        [HideInInspector]
        public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;
        public GameObject normalCamera;

        [Header("Ground & Air Detecting Stats")]
        [SerializeField]
        private float groundDectectionRayStartPoint = 0.5f;
        [SerializeField]
        private float minimumDistanceNeededToBeginFall = 1f;
        [SerializeField]
        private float groundDirectionRayDistance = 0.2f;
        private LayerMask ignoreForGroundCheck;
        public float inAirTimer;
        
        [Header("Movement Stats")]
        [SerializeField]
        private float movementSpeed = 5;
        [SerializeField]
        private float sprintSpeed = 7;
        [SerializeField]
        private float rotationSpeed = 10;
        [SerializeField]
        private float fallingSpeed = 80;
        
        private void Start()
        {
            _playerManager = GetComponent<PlayerManager>();
            rigidbody = GetComponent<Rigidbody>();
            _inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();

            _playerManager.isGrounded = true;
            ignoreForGroundCheck = ~(1 << 8 | 1 << 11);
        }



        #region Movement
        
        private Vector3 normalVector;
        private Vector3 targetPosition;

        /// <summary>
        /// 处理旋转逻辑
        /// </summary>
        /// <param name="delta"></param>
        private void HandleRotation(float delta)
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = _inputHandler.moveAmount;

            targetDir = cameraObject.forward * _inputHandler.vertical;
            targetDir += cameraObject.right * _inputHandler.horizontal;
            
            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
                targetDir = myTransform.forward;

            float rs = rotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);

            myTransform.rotation = targetRotation;
        }

        public void HandleMovement(float delta)
        {
            if (_inputHandler.rollFlag)
                return;

            if (_playerManager.isInteracting)
                return;
            
            moveDirection = cameraObject.forward * _inputHandler.vertical;
            moveDirection += cameraObject.right * _inputHandler.horizontal;
            moveDirection.Normalize();
            moveDirection.y = 0;

            float speed = movementSpeed;

            if (_inputHandler.sprintFlag && _inputHandler.moveAmount > 0.5)
            {
                speed = sprintSpeed;
                _playerManager.isSprinting = true;
                moveDirection *= speed;
            }
            else
            {
                if(_inputHandler.moveAmount < 0.5)
                {
                    moveDirection *= movementSpeed;
                    _playerManager.isSprinting = false;
                }
                else
                {
                    moveDirection *= speed;
                    _playerManager.isSprinting = false;
                }
            }

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = projectedVelocity;

            animatorHandler.UpdateAnimatorValues(_inputHandler.moveAmount, 0, _playerManager.isSprinting);

            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }
        }

        public void HandleRollingAndSprinting(float delta)
        {
            if (animatorHandler.anim.GetBool("isInteracting"))
                return;

            if (_inputHandler.rollFlag)
            {
                moveDirection = cameraObject.forward * _inputHandler.vertical;
                moveDirection += cameraObject.right * _inputHandler.horizontal;

                if (_inputHandler.moveAmount > 0)
                {
                    animatorHandler.PlayTargetAnimation("Rolling", true);
                    moveDirection.y = 0;
                    Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                    myTransform.rotation = rollRotation;
                }
                else
                {
                    animatorHandler.PlayTargetAnimation("Backstep", true);
                }
            }
        }

        public void HandleFalling(float delta, Vector3 moveDirection)
        {
            _playerManager.isGrounded = false;
            RaycastHit hit;
            Vector3 origin = myTransform.position;
            origin.y += groundDectectionRayStartPoint;

            if (Physics.Raycast(origin, myTransform.forward, out hit, 0.4f))
            {
                moveDirection = Vector3.zero;
            }

            if (_playerManager.isInAir)
            {
                rigidbody.AddForce(-Vector3.up * fallingSpeed);
                rigidbody.AddForce(moveDirection * fallingSpeed / 5f);
            }

            Vector3 dir = moveDirection;
            dir.Normalize();
            origin = origin + dir * groundDirectionRayDistance;

            targetPosition = myTransform.position;

            Debug.DrawRay(origin, -Vector3.up * minimumDistanceNeededToBeginFall, Color.red, 0.1f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, minimumDistanceNeededToBeginFall, ignoreForGroundCheck))
            {
                normalVector = hit.normal;
                Vector3 tp = hit.point;
                _playerManager.isGrounded = true;
                targetPosition.y = tp.y;

                if (_playerManager.isInAir)
                {
                    if (inAirTimer > 0.5f)
                    {
                        Debug.Log("滞空" + inAirTimer + "秒");
                        animatorHandler.PlayTargetAnimation("Land", true);
                        inAirTimer = 0;
                    }
                    else
                    {
                        animatorHandler.PlayTargetAnimation("Empty", false);
                        inAirTimer = 0;
                    }

                    _playerManager.isInAir = false;
                }
            }
            else
            {
                if (_playerManager.isGrounded)
                {
                    _playerManager.isGrounded = false;
                }

                if (_playerManager.isInAir == false)
                {
                    if (_playerManager.isInteracting == false)
                    {
                        animatorHandler.PlayTargetAnimation("Falling", true);
                    }

                    Vector3 vel = rigidbody.velocity;
                    vel.Normalize();
                    rigidbody.velocity = vel * (movementSpeed / 2);
                    _playerManager.isInAir = true;
                }
            }

            //if (_playerManager.isGrounded)
            //{
                if (_playerManager.isInteracting || _inputHandler.moveAmount > 0)
                {
                    myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, Time.deltaTime / 0.1f);
                }
                else
                {
                    myTransform.position = targetPosition;
                }
            //}
        }

        #endregion
    }
}

