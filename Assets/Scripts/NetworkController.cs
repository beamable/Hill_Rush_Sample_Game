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
    public int framesPerSecond = 20;

    public float LastTickTime { get; set; }
    public float TicksPerSecond { get; set; }

    public Queue<Message> _messages;
    public SimClient SimClient => _sim;

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
        _sim = new SimClient(new SimNetworkEventStream(roomId), framesPerSecond, 4);
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
    }

    private void HandleOnTick(long tick)
    {
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
            // Debug.Log("Player sent message. " + message);
            _messages.Enqueue(message);
        });

        _messages.Enqueue(new PlayerJoinedMessage
        {
            FromPlayer = dbidNumber
        });
    }


    private void HandleOnDisconnect(string dbid)
    {
        Debug.Log("Sim client has disconnection from " + dbid);
    }

    private void FixedUpdate()
    {
        _sim?.Update();
    }

    public void SendMessage(Message message)
    {
        _sim.SendEvent("message", message);
    }
}
