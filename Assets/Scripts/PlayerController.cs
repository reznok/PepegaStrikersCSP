using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using QFSW.QC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] StrikeIndicator _strikeIndicator;

    private void Awake()
    {
        InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
    }


    private void OnDestroy()
    {
        InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
        InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
    }

    private void TimeManager_OnTick()
    {
        if (base.IsOwner)
        {
            Reconciliation(default, false);
        }

        if (IsServer)
        {
            RHitCore(default, true);
        }
    }

    private void TimeManager_OnPostTick()
    {
        if (base.IsServer)
        {
            Rigidbody2D rb = GameObject.FindObjectOfType<CoreController>().GetComponent<Rigidbody2D>();
            ReconcileData rd = new ReconcileData(rb.position, rb.transform.rotation, rb.velocity);
            Reconciliation(rd, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (base.IsOwner)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StrikeData md = new();
                md.Strike = true;
                RHitCore(md, false);
            }
        }
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
    public struct StrikeData : IReplicateData
    {
        public bool Strike;
        public StrikeData(bool strike)
        {
            Strike = strike;
            _tick = 0;
        }

        private uint _tick;
        public void Dispose() { }
        public uint GetTick() => _tick;
        public void SetTick(uint value) => _tick = value;
    }

    [Replicate]
    private void RHitCore(StrikeData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        if (md.Strike)
        {
            Debug.Log("Hit Core!");
            GameObject.FindObjectOfType<CoreController>().ProcessHit(replaying);
        }  
    }

    [Reconcile]
    private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
    {
        Rigidbody2D rb = GameObject.FindObjectOfType<CoreController>().GetComponent<Rigidbody2D>();
        rb.velocity = rd.Velocity;
        rb.position = rd.Position;
    }

}
