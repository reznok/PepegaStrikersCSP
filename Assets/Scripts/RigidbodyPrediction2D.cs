using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using QFSW.QC;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
* 
* See TransformPrediction.cs for more detailed notes.
* 
*/



public class RigidbodyPrediction2D : NetworkBehaviour
{
    #region Types.
    public struct MoveData :IReplicateData
    {
        public float Horizontal;
        public float Vertical;
        public MoveData(float horizontal, float vertical)
        {
            Horizontal = horizontal;
            Vertical = vertical;
            _tick = 0;
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
        public Vector3 Velocity;
        public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }
    #endregion

    #region Serialized.

    //[SerializeField]
    // private float _moveRate = 100f; // Calc'd now

    [SerializeField]
    private float _maxSpeed = 8f;
    #endregion

    #region Private.
    /// <summary>
    /// Rigidbody on this object.
    /// </summary>
    private Rigidbody2D _rigidbody;
    /// <summary>
    /// Next time a jump is allowed.
    /// </summary>
    private float _nextJumpTime;
    private static Scene _simulationScene;

    #endregion
    private void Awake()
    {

        _rigidbody = GetComponent<Rigidbody2D>();
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;

    }

    private void OnDestroy()
    {
        if (InstanceFinder.TimeManager != null)
        {
            InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
        }
    }

    private void Update()
    {
        if (base.IsOwner)
        {
            if (Input.GetKeyDown(KeyCode.Space) && Time.time > _nextJumpTime)
            {
                _nextJumpTime = Time.time + 1f;
            }
        }
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
            CheckInput(out MoveData md);
            Move(md, false);
        }
        if (base.IsServer)
        {
            Move(default, true);
        }
    }


    private void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity);
            Reconciliation(rd, true);
        }
    }

    private void CheckInput(out MoveData md)
    {
        md = default;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal == 0f && vertical == 0f)
            return;

        md = new MoveData(horizontal, vertical);
    }

    [Replicate]
    private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        //Add extra gravity for faster falls.
        float moveRate = _maxSpeed * 10;
        Vector3 forces = new Vector3(md.Horizontal, md.Vertical) * moveRate;
        _rigidbody.AddForce(forces);
        _rigidbody.velocity = Vector2.ClampMagnitude(_rigidbody.velocity, _maxSpeed);

    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = rd.Position;
        transform.rotation = rd.Rotation;
        _rigidbody.velocity = rd.Velocity;
    }
}


