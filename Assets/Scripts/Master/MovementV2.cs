using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using DG.Tweening;

namespace masterland.Master
{
    public class MovementV2 : MasterComponent
    {
        [Header("Stats")]
        public float MoveSpeed = 2f;
        public float SprintSpeed = 5.335f;
        public float RotationSmoothTime = 12f;
        public float SpeedChangeRate = 10.0f;
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float FallTimeout = 0.15f;
        public bool Grounded = true;
        public bool IsSliding = false;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        private float _targetRotation = 0.0f;
        public float VerticalVelocity;
        public float AnimationBlend;
        public float AnimationBlendX;

        private float _terminalVelocity = 53.0f;
        public float _maxSlopeAngle = 45f;

        [HideInInspector] public CharacterController Controller;

        public float ActionTime = 0.5f;
        public float RunSpeedTime;
        public float JumpTime;
        public float CurrentSpeed;
        public float CurrentSpeedX;
        [SerializeField] private Vector3 _initRotateForward;
        Tween currentTween = null;
        private bool _isTurning = false;
        private bool _isJumping = false;
        [SerializeField] private float _nearRoundCheck = 0.05f;


        public override void Awake()
        {
            base.Awake();
            Controller = GetComponent<CharacterController>();
        }

        public override void OnStartClient()
        {
            Controller.enabled = (base.IsServerInitialized || base.IsOwner);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Controller.enabled = true;
        }

        public override void Setup(Master master)
        {
            base.Setup(master);
        }

        public void LocoMovement(ReplicateData rd, float deltaTime)
        {
            if(rd.LockOn)
                return;

            float rootSpeed = rd.Sprint && !_master.State.IsAction ? SprintSpeed : MoveSpeed;
            Vector3 deltaPositionFilter = _master.State.IsAction ? rd.DeltaPosition : Vector3.zero;
            if (Vector3.Distance(deltaPositionFilter, _master.AnimatorHook.DeltaPosition) > 1f)
            {
                deltaPositionFilter = _master.AnimatorHook.DeltaPosition;
            }

            if (rd.Sprint && _isJumping)
                rootSpeed = SprintSpeed / 1.2f;

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, rootSpeed, deltaTime * 5f);
            RunSpeedTime = rd.Sprint && !_master.State.IsAction || _master.State.IsActionKeepSpeed ? RunSpeedTime += deltaTime : 0;

            if ((rd.Move == Vector2.zero || _master.State.IsAction) && !_master.State.IsActionKeepSpeed) CurrentSpeed = 0.0f;
            if (_master.State.IsActionKeepSpeed)
                CurrentSpeed = MoveSpeed;

            AnimationBlend = Mathf.Lerp(AnimationBlend, CurrentSpeed / SprintSpeed, deltaTime * SpeedChangeRate);
            if (AnimationBlend < 0.01f) AnimationBlend = 0f;

            Vector3 inputDirection = new Vector3(rd.Move.x, 0.0f, rd.Move.y).normalized;

            if (rd.Move != Vector2.zero && !_master.State.IsHardAction)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + rd.CameraAngle;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, _targetRotation, 0.0f), RotationSmoothTime * deltaTime);
            }

            Vector3 targetDirection = _master.State.IsAction ? Vector3.zero : Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            Vector3 deltaPosition = _master.State.IsActionKeepSpeed ? (deltaPositionFilter * 2) + transform.forward * 0.1f : deltaPositionFilter;

            Vector3 deltaMovement;

            if (IsOnSteepSlope(out _, out Vector3 slopeDirection))
            {
                Vector3 fallMovement = slopeDirection * -VerticalVelocity * deltaTime;
                deltaMovement = fallMovement;

            }
            else
            {
                deltaMovement = targetDirection.normalized * (CurrentSpeed * deltaTime) +
                                     deltaPosition +
                                     new Vector3(0.0f, VerticalVelocity, 0.0f) * deltaTime;
            }

            if (_master.State.IsHardAction)
                RunSpeedTime = 0;

            if (Controller.enabled)
                Controller.Move(deltaMovement);

            if (IsClientInitialized)
                _master.Animation.LocoMovement(AnimationBlend);
        }

        public void LockOnMovement(ReplicateData rd, float deltaTime)
        {
            RunSpeedTime = 0;

            float rootSpeed = rd.Sprint ? SprintSpeed / 2.3f : MoveSpeed / 2f;

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, rootSpeed * rd.Move.y, deltaTime * 10f);
            CurrentSpeedX = Mathf.Lerp(CurrentSpeedX, rootSpeed * rd.Move.x, deltaTime * 10f);

            AnimationBlend = CurrentSpeed / (SprintSpeed / 3);
            AnimationBlendX = CurrentSpeedX / (SprintSpeed / 3);

            rd.CameraForward.y = 0f;
            float targetRotation = Mathf.Atan2(rd.CameraForward.x, rd.CameraForward.z) * Mathf.Rad2Deg;

            if (rd.Move != Vector2.zero)
            {
                if(!_master.State.IsDodge)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRotation, 0f), RotationSmoothTime * (float)TimeManager.TickDelta);
            }

            Vector3 moveDir = transform.forward * rd.Move.y + transform.right * rd.Move.x;
            Vector3 deltaPosition = _master.State.IsAction ? rd.DeltaPosition : Vector3.zero;
           
            if (Vector3.Distance(deltaPosition, _master.AnimatorHook.DeltaPosition) > 1f)
                deltaPosition = _master.AnimatorHook.DeltaPosition;

            Vector3 targetDirection = _master.State.IsAction ? Vector3.zero : (moveDir * rootSpeed * deltaTime);

            if (Controller.enabled)
                Controller.Move(targetDirection + deltaPosition +
                                    new Vector3(0.0f, VerticalVelocity, 0.0f) * deltaTime);

            if (IsClientInitialized)
                _master.Animation.LockOnMovement(AnimationBlend, AnimationBlendX);
        }

        public void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers);

            if (Grounded && IsOnSteepSlope(out Vector3 slopeNormal, out Vector3 slopeDirection))
            {
                IsSliding = true;
                Grounded = false;
            }
            else
            {
                IsSliding = false;
            }
        }

        private bool IsOnSteepSlope(out Vector3 slopeNormal, out Vector3 slopeDirection)
        {
            slopeNormal = Vector3.up;
            slopeDirection = Vector3.zero;

            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 2f, GroundLayers))
            {
                slopeNormal = hit.normal;
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle >= Controller.slopeLimit && !_isJumping)
                {
                    slopeDirection = Vector3.ProjectOnPlane(Vector3.down, slopeNormal);
                    return true;
                }
            }

            return false;
        }

        public void JumpAndGravity(ReplicateData rd, float delta)
        {
            if (rd.LockOn)
                return;

            float distanceToGround;

            if (Grounded)
            {
                if (VerticalVelocity < 0.0f)
                {
                    VerticalVelocity = -2f;

                    if (_isJumping)
                    {
                        _isJumping = false;
                        _master.Animation.SetJump(false);
                        if (CurrentSpeed != 0)
                            CurrentSpeed = 0;
                    }
                }

                if (rd.Jump && !_master.State.IsAction)
                {
                    if (!_isJumping)
                    {
                        _isJumping = true;
                        _master.Animation.SetJump(true);
                         VerticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    }
                }
            }
            else
            {
                if (VerticalVelocity < _terminalVelocity)
                {
                    VerticalVelocity += Gravity * delta;
                }

                if (IsNearGround(_nearRoundCheck, out distanceToGround))
                {
                    if (_isJumping)
                    {
                        _isJumping = false;
                        Debug.Log("SetJump false when near ground, distance: " + distanceToGround);
                        _master.Animation.SetJump(false);
                    }
                }
            }

            JumpTime = Grounded ? 0 : JumpTime += delta;
        }

        private bool IsNearGround(float threshold, out float distance)
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, threshold, GroundLayers))
            {
                distance = hit.distance;
                return true;
            }
            distance = float.MaxValue;
            return false;
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

    }
}