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
    public class Movement : MasterComponent
    {
        public struct MoveData : IReplicateData
        {
            public Vector2 Move;
            public Vector3 DeltaPostion;
            public float CameraAngle;
            public bool Sprint;
            public bool Jump;
            public bool LockOn;
            public Vector3 CameraForward;

            public MoveData(Vector2 move, Vector3 deltaPostion, float cameraAngle,
                                bool sprint, bool jump, bool lockOn, Vector3 cameraForward) : this()
            {
                Move = move;
                DeltaPostion = deltaPostion;
                CameraAngle = cameraAngle;
                Sprint = sprint;
                Jump = jump;
                LockOn = lockOn;
                CameraForward = cameraForward;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
        public struct ReconcileData : IReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public float VerticalVelocity;
            public bool Grounded;
            public float RunSpeedTime;
            public float JumpTime;
            public float CurrentSpeed;
            public float CurrentSpeedX;
            public ReconcileData(Vector3 position, Quaternion rotation,
                                float verticalVelocity, bool grounded,
                                float runSpeedTime, float jumpTime,
                                float currentSpeed, float currentSpeedX) : this()
            {
                Position = position;
                Rotation = rotation;
                VerticalVelocity = verticalVelocity;
                Grounded = grounded;
                RunSpeedTime = runSpeedTime;
                JumpTime = jumpTime;
                CurrentSpeed = currentSpeed;
                CurrentSpeedX = currentSpeedX;
                _tick = 0;
            }

            private uint _tick;
            public void Dispose() { }
            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
        }
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
        private float _animationBlend;
        private float _animationBlendX;
        private float _targetRotation = 0.0f;
        [SerializeField] private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        public float _maxSlopeAngle = 45f;
      

        [HideInInspector] public CharacterController Controller;

        public MoveData MasterMoveData;
        public float ActionTime = 0.5f;
        public float RunSpeedTime;
        public float JumpTime;
        public float CurrentSpeed;
        public float CurrentSpeedX;
        [SerializeField] private Vector3 _initRotateForward;
        Tween currentTween = null;
        private bool _isTurning = false;
        private bool _isJumping = false;
        private bool _isDelayJump = false;
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

        public override void OnTick()
        {
            base.OnTick();

            if (base.IsOwner)
            {
                CheckInput(out MoveData md);
                Move(md);
            }
            else if (base.IsServerInitialized)
            {
                Move(default);
            }
        }

        public override void OnPostTick()
        {
            base.OnPostTick();
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation,
                                                     _verticalVelocity, Grounded,
                                                    RunSpeedTime, JumpTime, CurrentSpeed, CurrentSpeedX);
            Reconciliation(rd);
        }


        private void CheckInput(out MoveData md)
        {
            md = default;

            md = new MoveData()
            {
                Move = _master.Input.MoveDirection,
                Jump = _master.Input.PlayJump,
                Sprint = _master.Input.PlaySprint && !_master.State.IsTired,
                DeltaPostion = _master.State.IsAction ? _master.AnimatorHook.DeltaPosition : Vector3.zero,
                CameraAngle = CameraManager.Instance.GetAngle(),
                LockOn = _master.Input.PlayLockOn && Grounded,
                CameraForward = Camera.main.transform.forward
            };
            _master.Input.PlayJump = false;
        }

        [Replicate]
        private void Move(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {

            GroundedCheck();
            if (md.LockOn && Grounded)
            {
                LockOnMovement(md, (float) TimeManager.TickDelta);
            }
            else
            {
                LocoMovement(md, (float) TimeManager.TickDelta);
                JumpAndGravity(md, (float) TimeManager.TickDelta);
                _initRotateForward = transform.forward;
            }
            _master.State.IsBlock = md.LockOn && Grounded;


            MasterMoveData = md;
        }



        private void LocoMovement(MoveData md, float deltaTime)
        {
            //md.Sprint = md.Sprint && !_master.State.IsTired;
            float rootSpeed = md.Sprint && !_master.State.IsAction? SprintSpeed : MoveSpeed;
            if (md.Sprint && _isJumping)
                rootSpeed = SprintSpeed/1.2f;

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, rootSpeed, deltaTime * 5f);
            RunSpeedTime = md.Sprint && !_master.State.IsAction || _master.State.IsActionKeepSpeed ? RunSpeedTime += deltaTime : 0;

            if ((md.Move == Vector2.zero || _master.State.IsAction) && !_master.State.IsActionKeepSpeed) CurrentSpeed = 0.0f;
            if (_master.State.IsActionKeepSpeed)
                CurrentSpeed = MoveSpeed;


            _animationBlend = Mathf.Lerp(_animationBlend, CurrentSpeed / SprintSpeed, deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(md.Move.x, 0.0f, md.Move.y).normalized;

            if (md.Move != Vector2.zero && !_master.State.IsHardAction)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + md.CameraAngle;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0.0f, _targetRotation, 0.0f), RotationSmoothTime * deltaTime);
            }

            Vector3 targetDirection = _master.State.IsAction ? Vector3.zero : Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            Vector3 deltaPosition = _master.State.IsActionKeepSpeed ? (md.DeltaPostion * 2) + transform.forward * 0.1f : md.DeltaPostion;

            Vector3 deltaMovement;

            if (IsOnSteepSlope(out _, out Vector3 slopeDirection))
            {
                Vector3 fallMovement = slopeDirection * -_verticalVelocity * deltaTime;
                deltaMovement = fallMovement;

            }
            else
            {
                deltaMovement = targetDirection.normalized * (CurrentSpeed * deltaTime) +
                                     deltaPosition +
                                     new Vector3(0.0f, _verticalVelocity, 0.0f) * deltaTime;
            }

            if(_master.State.IsHardAction)
                RunSpeedTime =0;

            if (Controller.enabled)
                Controller.Move(deltaMovement);

            if (IsClientInitialized)
                _master.Animation.LocoMovement(_animationBlend);
        }

        private void LockOnMovement(MoveData md, float deltaTime)
        {
            RunSpeedTime = 0;

            float rootSpeed = md.Sprint ? SprintSpeed / 2.3f : MoveSpeed / 2f;

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, rootSpeed * md.Move.y, deltaTime * 10f);
            CurrentSpeedX = Mathf.Lerp(CurrentSpeedX, rootSpeed * md.Move.x, deltaTime * 10f);

            _animationBlend = CurrentSpeed / (SprintSpeed / 3);
            _animationBlendX = CurrentSpeedX / (SprintSpeed / 3);

            Vector3 characterForward = _initRotateForward;
            Vector3 cameraForward = md.CameraForward;

            Vector2 characterForward2D = new Vector2(characterForward.x, characterForward.z).normalized;
            Vector2 cameraForward2D = new Vector2(cameraForward.x, cameraForward.z).normalized;

            float angleBetween = Vector2.Angle(characterForward2D, cameraForward2D);
            float direction = Mathf.Sign(Vector3.Cross(characterForward, cameraForward).y);

            md.CameraForward.y = 0f;
            float targetRotation = Mathf.Atan2(md.CameraForward.x, md.CameraForward.z) * Mathf.Rad2Deg;

            if (md.Move == Vector2.zero && angleBetween > 70 && !_isTurning && !_master.State.IsDodge && !_master.State.IsHardAction)
            {
                _isTurning = true;
                string turnDirection = direction > 0 ? "TurnRight" : "TurnLeft";
                if (IsOwner) _master.Animation.Server_PlayAction(turnDirection);
                currentTween = transform.DORotate(new Vector3(0f, targetRotation, 0f), 0.8f)
                    .SetEase(Ease.OutQuad)
                    .OnStart(() => { })
                    .OnComplete(() =>
                    {
                        _isTurning = false;
                        _initRotateForward = transform.forward;
                    });
            }
            else if (md.Move != Vector2.zero && !_master.State.IsDodge)
            {
                if (currentTween != null && currentTween.IsActive() && !currentTween.IsComplete())
                {
                    currentTween.Kill();
                    _isTurning = false;

                    if (IsOwner)
                        _master.Animation.Server_PlayAction("Empty");

                }
                _initRotateForward = transform.forward;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRotation, 0f), RotationSmoothTime * deltaTime);
            }

            Vector3 moveDir = transform.forward * md.Move.y + transform.right * md.Move.x;
            Vector3 deltaPosition = _master.State.IsAction ? md.DeltaPostion : Vector3.zero;

            Vector3 targetDirection = _master.State.IsAction ? Vector3.zero : (moveDir * rootSpeed * deltaTime);

            if (Controller.enabled)
                Controller.Move(targetDirection + deltaPosition +
                                    new Vector3(0.0f, _verticalVelocity, 0.0f) * deltaTime);

            if (IsClientInitialized)
                _master.Animation.LockOnMovement(_animationBlend, _animationBlendX);
        }




        [Reconcile]
        private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _verticalVelocity = rd.VerticalVelocity;
            Grounded = rd.Grounded;
            RunSpeedTime = rd.RunSpeedTime;
            JumpTime = rd.JumpTime;
            CurrentSpeed = rd.CurrentSpeed;
            CurrentSpeedX = rd.CurrentSpeedX;
        }


        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers) && !_isDelayJump;

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

        private void JumpAndGravity(MoveData md, float delta)
        {
            if (md.LockOn)
                return;

            float distanceToGround;

            if (Grounded)
            {
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;

                    if (_isJumping)
                    {
                        _isJumping = false;
                       _master.Animation.SetJump(false);
                       if(CurrentSpeed!=0) 
                            CurrentSpeed =0; 
                    }
                }

                if (md.Jump && !_master.State.IsAction)
                {
                    if (!_isJumping)
                    {
                        StartCoroutine(DelayedJump(JumpHeight, md.Move== Vector2.zero ? 0.2f : 0.1f));
                    }
                }
            }
            else
            {
                if (_verticalVelocity < _terminalVelocity)
                {
                    _verticalVelocity += Gravity * delta;
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

            // _master.Animation.SetSliding(IsSliding);
            // _master.Animation.SetGrounded(Grounded);
        }

        IEnumerator DelayedJump(float jumpFactor, float delayTime)
        {
            _isJumping = true;
            _isDelayJump =true;
            _master.Animation.SetJump(true);
            yield return new WaitForSeconds(delayTime);
            _isDelayJump =false;
            _verticalVelocity = Mathf.Sqrt(jumpFactor * -2f * Gravity);

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