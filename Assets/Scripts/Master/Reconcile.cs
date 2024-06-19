using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using masterland.Master;
using UnityEngine;

namespace masterland.Master
{
    public struct ReplicateData : IReplicateData
    {
        //movement 
        public Vector2 Move;
        public Vector3 DeltaPosition;
        public float CameraAngle;
        public bool Sprint;
        public bool Jump;
        public bool LockOn;
        public Vector3 CameraForward;

        public ReplicateData(Vector2 move, Vector3 deltaPostion, float cameraAngle,
                            bool sprint, bool jump, bool lockOn, Vector3 cameraForward) : this()
        {
            Move = move;
            DeltaPosition = deltaPostion;
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
        //state 
        public bool IsAction;
        public bool IsActionKeepSpeed;
        public bool IsHardAction;
        public bool IsDodge;
        public bool IsGetHit;
        public bool IsGrounded;
        public bool IsGuard;
        public bool IsTired;
        public bool IsBlock;
        
        //Movement 
        public Vector3 Position;
        public Quaternion Rotation;
        public float VerticalVelocity;
        public float RunSpeedTime;
        public float JumpTime;
        public float CurrentSpeed;
        public float CurrentSpeedX;
        public float AnimationBlend;
        public float AnimationBlendX;

        public ReconcileData(bool isAction, bool isActionKeepSpeed, bool isHardAction, bool isDodge, bool isGetHit,
                            bool isGrounded, bool isGuard, bool isTired, bool isBlock,
                            Vector3 position, Quaternion rotation, float verticalVelocity,
                            float runSpeedTime, float jumpTime, float currentSpeed, float currentSpeedX,
                            float animationBlend, float animationBlendX
                            ) : this()
        {
            //state
            IsAction = isAction;
            IsActionKeepSpeed = isActionKeepSpeed;
            IsHardAction = isHardAction;
            IsDodge = isDodge;
            IsGetHit = isGetHit;
            IsGrounded = isGrounded;
            IsGuard = isGuard;
            IsTired = isTired;
            IsBlock = isBlock;

            //movement 
            Position = position;
            Rotation = rotation;
            VerticalVelocity = verticalVelocity;
            RunSpeedTime = runSpeedTime;
            JumpTime = jumpTime;
            CurrentSpeed = currentSpeed;
            CurrentSpeedX = currentSpeedX;
            AnimationBlend = animationBlend;
            AnimationBlendX = animationBlendX;

            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
    public class Reconcile : MasterComponent
    {
        
        public ReplicateData ReplicateData;
        private bool isPrediction;
       
        public override void Awake() 
        {
            base.Awake();
            isPrediction = GetComponent<NetworkObject>().EnablePrediction;
        }

        public override void OnTick()
        {
            base.OnTick();

            

            if (base.IsOwner)
            {
                CheckInput(out ReplicateData md);
                Replication(md);
            }
            else if (base.IsServerInitialized)
            {
                Replication(default);
            }
        }

        public override void OnPostTick()
        {
            base.OnPostTick();
            CreateReconcile();
        }


        private void CheckInput(out ReplicateData md)
        {
            md = default;

            md = new ReplicateData()
            {
                Move = _master.Input.MoveDirection,
                DeltaPosition = _master.AnimatorHook.DeltaPosition,
                Jump = _master.Input.PlayJump,
                Sprint = _master.Input.PlaySprint && !_master.State.IsTired,
                CameraAngle = CameraManager.Instance.GetAngle(),
                LockOn = _master.Input.PlayLockOn && _master.State.IsGrounded,
                CameraForward = Camera.main.transform.forward
            };
            _master.Input.PlayJump = false;
        }




        public override void CreateReconcile()
        {
            ReconcileData rd = new ReconcileData(
                _master.State.IsAction, _master.State.IsActionKeepSpeed,
                _master.State.IsHardAction, _master.State.IsDodge, _master.State.IsGetHit,
                _master.State.IsGrounded, _master.State.IsGuard, _master.State.IsTired, _master.State.IsBlock,
                transform.position, transform.rotation, _master.Movement.VerticalVelocity,
                _master.Movement.RunSpeedTime, _master.Movement.JumpTime, _master.Movement.CurrentSpeed, _master.Movement.CurrentSpeedX,
                _master.Movement.AnimationBlend, _master.Movement.AnimationBlendX
            );

            Reconciliation(rd);
        }

        [Replicate]
        private void Replication(ReplicateData replicateData, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            ReplicateData = replicateData;

            #if !UNITY_EDITOR
            if(IsServerInitialized && !isPrediction)
                return;
            #endif
            
            _master.Movement.GroundedCheck();

            if (replicateData.LockOn && _master.State.IsGrounded)
            {
                _master.Movement.LockOnMovement(replicateData, (float) TimeManager.TickDelta);
            }
            else
            {
                _master.Movement.LocoMovement(replicateData, (float) TimeManager.TickDelta);
                _master.Movement.JumpAndGravity(replicateData, (float) TimeManager.TickDelta);
            }
            _master.State.IsBlock = replicateData.LockOn && _master.State.IsGrounded;
        }


        [Reconcile]
        private void Reconciliation(ReconcileData rd, Channel channel = Channel.Unreliable)
        {
            //state
            _master.State.IsAction = rd.IsAction;
            _master.State.IsActionKeepSpeed = rd.IsActionKeepSpeed;
            _master.State.IsHardAction = rd.IsHardAction;
            _master.State.IsDodge = rd.IsDodge;
            _master.State.IsGetHit = rd.IsGetHit;
            _master.State.IsGrounded = rd.IsGrounded;
            _master.State.IsGuard = rd.IsGuard;
            _master.State.IsTired = rd.IsTired;
            _master.State.IsBlock = rd.IsBlock;

            //movement
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _master.Movement.VerticalVelocity = rd.VerticalVelocity;
            _master.Movement.RunSpeedTime = rd.RunSpeedTime;
            _master.Movement.JumpTime = rd.JumpTime;
            _master.Movement.CurrentSpeed  =rd.CurrentSpeed;
            _master.Movement.CurrentSpeedX = rd.CurrentSpeedX;
            // _master.Movement.AnimationBlend = rd.AnimationBlend;
            // _master.Movement.AnimationBlendX = rd.AnimationBlendX;
        }


    }
}
