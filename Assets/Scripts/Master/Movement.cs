using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;

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
            public Vector3 TargetLockOn;

            public MoveData(Vector2 move, Vector3 deltaPostion, float cameraAngle, 
                                bool sprint, bool jump, bool lockOn, Vector3 targetLockOn) : this()
            {
                Move = move;
                DeltaPostion = deltaPostion;
                CameraAngle = cameraAngle;
                Sprint = sprint;
                Jump = jump;
                LockOn = lockOn;
                TargetLockOn = targetLockOn;
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
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;
        private float _animationBlend;
        private float _animationBlendX;
        private float _targetRotation = 0.0f;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;


       [HideInInspector] public CharacterController Controller;
    
        public MoveData MasterMoveData;
        public float ActionTime = 0.5f;
        public float RunSpeedTime;
        public float JumpTime;
        public float CurrentSpeed;
        public float CurrentSpeedX;

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
            //CreateReconcile();
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
                TargetLockOn = _master.Combat.CurrentTargetLockOn == null ? Vector3.zero : _master.Combat.CurrentTargetLockOn.transform.position
            };
            _master.Input.PlayJump = false;
        }

        [Replicate]
        private void Move(MoveData md, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
      
                GroundedCheck();
                if (md.LockOn && Grounded) 
                {
                    LockOnMovement(md, (float)base.TimeManager.TickDelta);
                }
                else {
                    LocoMovement(md, (float)base.TimeManager.TickDelta);
                    JumpAndGravity(md, (float)base.TimeManager.TickDelta);
                } 
                 _master.State.IsBlock = md.LockOn && Grounded;
                MasterMoveData = md;  
        }

     

        private void LocoMovement(MoveData md, float deltaTime)
        {
            //md.Sprint = md.Sprint && !_master.State.IsTired;
            float rootSpeed = md.Sprint && !_master.State.IsAction? SprintSpeed : MoveSpeed;
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

            Vector3 deltaMovement = targetDirection.normalized * (CurrentSpeed * deltaTime) +
                                    deltaPosition +
                                    new Vector3(0.0f, _verticalVelocity, 0.0f) * deltaTime;
            
            if(Controller.enabled)
                Controller.Move(deltaMovement);
           
            if(IsClientInitialized)
                _master.Animation.LocoMovement(_animationBlend);
        }

        private void LockOnMovement(MoveData md, float deltaTime)
        {
            RunSpeedTime =0;

            float rootSpeed = md.Sprint? SprintSpeed/3  : MoveSpeed/3;

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, rootSpeed*md.Move.y, deltaTime * 10f);
            CurrentSpeedX = Mathf.Lerp(CurrentSpeedX, rootSpeed*md.Move.x, deltaTime* 10f);

            _animationBlend = CurrentSpeed/(SprintSpeed/3);
            _animationBlendX = CurrentSpeedX/(SprintSpeed/3);

            Vector3 from = md.TargetLockOn - transform.position;
            from.y = 0;
            Quaternion rotation = Quaternion.LookRotation(from);
            if(!_master.State.IsDodge && !_master.State.IsHardAction)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.2f);

            Vector3 moveDir = transform.forward * md.Move.y + transform.right * md.Move.x;

            if(md.Move.y != 0 && md.Move.x != 0)
                rootSpeed=((Mathf.Abs(md.Move.y) + Mathf.Abs(md.Move.x))/2 * rootSpeed);
            
            Vector3 deltaPosition = _master.State.IsAction? md.DeltaPostion : Vector3.zero;
            
            Vector3 targetDirection = _master.State.IsAction? Vector3.zero : (moveDir * rootSpeed * deltaTime);
           
            if(Controller.enabled)
                Controller.Move(targetDirection + deltaPosition +
                                    new Vector3(0.0f, _verticalVelocity, 0.0f) * deltaTime);

             if(IsClientInitialized)
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
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers);
        }

        private void JumpAndGravity(MoveData md, float delta)
        {
            if(md.LockOn)
                return;

            float jumpFactor = _master.Weapon.GetTypeId() != 0 ? JumpHeight * 1.5f : JumpHeight;

            // if (Grounded && md.Jump && !_master.State.IsAction)
            //     _verticalVelocity = Mathf.Sqrt(jumpFactor * -2f * Gravity);

            JumpTime = Grounded ? 0 : JumpTime += delta;

            _master.Animation.SetGrounded(Grounded);

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * delta;
            }
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