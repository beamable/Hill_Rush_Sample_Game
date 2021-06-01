using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Experimental.Api.Sim;
using BeamableExtensions;
using Simulation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class NetworkController : MonoBehaviour
{
    private SimClient _sim;
    public string roomId;

    public static string roomIdOverride;

    public SimulationLog Log;
    public long LocalDbid;

    public const int NetworkFramesPerSecond = 20; // TODO: Un-hardcode this at the server level
    public static long HighestSeenNetworkFrame;
    public static bool NetworkInitialized;

    public static float SimTime => HighestSeenNetworkFrame / (float)NetworkFramesPerSecond;

    public async Task Init()
    {
        HighestSeenNetworkFrame = 0;
        NetworkInitialized = false;
        Log = new SimulationLog();
        roomId = string.IsNullOrEmpty(roomIdOverride) ? roomId : roomIdOverride;

        var beamable = await API.Instance;

        LocalDbid = beamable.User.id;
        _sim = new SimClient(new SimNetworkEventStream(roomId), NetworkFramesPerSecond, 4);
        _sim.OnInit(HandleOnInit);
        _sim.OnConnect(HandleOnConnect);
        _sim.OnDisconnect(HandleOnDisconnect);
        _sim.OnTick(HandleOnTick);
    }

    private void HandleOnInit(string seed)
    {
        Debug.Log("Sim client has initialized " + seed);
        NetworkInitialized = true;
    }

    private void HandleOnTick(long tick)
    {
        HighestSeenNetworkFrame = tick;
        Log.AddMessage(tick, new TickMessage(tick));
    }

    private void HandleOnConnect(string dbid)
    {
        Debug.Log("Sim client has connection from " + dbid);

        // listen for messages from this player...
        var dbidNumber = long.Parse(dbid);

        // ListenForMessageFrom<PlayerSpawnCubeMessage>(dbid);
        // ListenForMessageFrom<PlayerDestroyAllMessage>(dbid);
        // ListenForMessageFrom<PlayerInputMessage>(dbid);
        _sim.On<PlayerInputMessage>(nameof(PlayerInputMessage), dbid, msg =>
        {
            var time = Time.realtimeSinceStartup;
            var startTime = msg.Time;
            var dt = time - startTime;
            Debug.Log("MESSAGE DELAY WAS " + dt);
        });
        // _sim.On<HashCheckMessage>(nameof(HashCheckMessage), dbid, hashCheck =>
        // {
        //     hashCheck.FromPlayer = dbidNumber;
        //     if (dbidNumber == LocalDbid) return;
        //     Debug.Log("Validating hash from " + dbid + " for tick " + hashCheck.ForTick);
        //     Log.EnqueueHashAssertion(hashCheck.ForTick, hashCheck.Hash);
        // });

        var joinMsg = new PlayerJoinedMessage
        {
            Tick = HighestSeenNetworkFrame,
            FromPlayer = dbidNumber
        };
        Log.AddMessage(joinMsg);
    }


    private void HandleOnDisconnect(string dbid)
    {
        Debug.Log("Sim client has disconnection from " + dbid);
    }

    private void FixedUpdate()
    {
        _sim?.Update();
    }

    public void SendNetworkMessage(Message message)
    {
        // message.Tick = HighestSeenNetworkFrame + 1; // this message belongs on the next tick...
        // Debug.Log("Sending message " + message.Tick);
        _sim.SendEvent(message.GetType().Name, message);
    }

    SimClient.EventCallback<string> ListenForMessageFrom<T>(string origin)
        where T : Message
    {
        var dbid = long.Parse(origin);
        return _sim.On<T>(typeof(T).Name, origin, message =>
        {
            message.FromPlayer = dbid;
            Log.AddMessage(message);
        });
    }

}
