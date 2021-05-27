using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Experimental.Api.Sim;
using BeamableExtensions;
using Simulation;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    private SimClient _sim;
    public string roomId;
    // public int framesPerSecond = 20;

    public float LastTickTime { get; set; }
    public float TicksPerSecond { get; set; }

    public Queue<Message> _messages;
    public SimClient SimClient => _sim;

    private float _lastUpdateTime;

    // Start is called before the first frame update
    void Start()
    {

    }

    public Queue<Message> Init()
    {
        if (_messages != null)
        {
            throw new InvalidOperationException("Cannot init the simulation twice.");
        }

        _messages = new Queue<Message>();
        _sim = new SimClient(new SimNetworkEventStream(roomId), SimFixedRateManager.NetworkFramesPerSecond, 4);
        _sim.OnInit(HandleOnInit);
        _sim.OnConnect(HandleOnConnect);
        _sim.OnDisconnect(HandleOnDisconnect);
        _sim.OnTick(HandleOnTick);


        return _messages;
    }

    private void HandleOnInit(string seed)
    {
        Debug.Log("Sim client has initialized " + seed);
        LastTickTime = Time.realtimeSinceStartup;
        SimFixedRateManager.NetworkInitialized = true;
    }

    private void HandleOnTick(long tick)
    {
        // SimFixedRateManager.AllowTick(tick);
        _messages.Enqueue(new TickMessage(tick));

        // SimFixedRateManager.AllowTick(tick);
        // var t = Time.realtimeSinceStartup;
        // var delta = t - LastTickTime;
        // TicksPerSecond = delta;
        // SharedInputData.timestep = (sfloat)TicksPerSecond;
        // Debug.Log(tick + " - " + TicksPerSecond);
        // LastTickTime = t;
    }

    private void HandleOnConnect(string dbid)
    {
        Debug.Log("Sim client has connection from " + dbid);

        // listen for messages from this player...
        var dbidNumber = long.Parse(dbid);
        _sim.On<Message>("message", dbid, message =>
        {
            message.FromPlayer = dbidNumber;
            Debug.Log("received sent message. " + message.Tick);
            _messages.Enqueue(message);
        });

        _messages.Enqueue(new PlayerJoinedMessage
        {
            Tick = SimFixedRateManager.HighestSeenNetworkTick,
            FromPlayer = dbidNumber
        });
    }


    private void HandleOnDisconnect(string dbid)
    {
        Debug.Log("Sim client has disconnection from " + dbid);
    }

    private void FixedUpdate()
    {
        // var now = Time.realtimeSinceStartup;
        // var nextUpdateAt = _lastUpdateTime + (SimFixedRateManager.NetworkFramesPerSecond);
        // if (now > nextUpdateAt)
        // {
        //     _sim?.Update();
        //     _lastUpdateTime = now;
        // }
        //
        _sim?.Update();

    }

    public void SendMessage(Message message)
    {
        message.Tick = SimFixedRateManager.HighestSeenNetworkTick + 1; // this message belongs on the next tick...
        Debug.Log("Sending message " + message.Tick);

        _sim.SendEvent("message", message);
    }
}
