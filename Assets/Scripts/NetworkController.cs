using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // void Awake()
    // {
    //     var world = World.All[0];
    //     var fixedGroup = world.GetOrCreateSystem<FixedStepSimulationSystemGroup>();
    //
    //     var gameSystem = world.GetOrCreateSystem<GameSystem>();
    //     var gameController = world.GetOrCreateSystem<GameController>();
    //     var inputSystem = world.GetOrCreateSystem<InputSystem>();
    //     var timeSystem = world.GetOrCreateSystem<SimWorldTimeSystem>();
    //
    //     // fixedGroup.AddSystemToUpdateList(gameSystem);
    //     // fixedGroup.AddSystemToUpdateList(gameController);
    //     // fixedGroup.AddSystemToUpdateList(inputSystem);
    //     // fixedGroup.AddSystemToUpdateList(timeSystem);
    //
    //     // fixedGroup.SortSystems();
    //     ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
    // }

    public async Task Init()
    {
        Log = new SimulationLog();
        roomId = string.IsNullOrEmpty(roomIdOverride) ? roomId : roomIdOverride;

        var beamable = await Beamable.API.Instance;

        LocalDbid = beamable.User.id;
        _sim = new SimClient(new SimNetworkEventStream(roomId), SimFixedRateManager.NetworkFramesPerSecond, 4);
        _sim.OnInit(HandleOnInit);
        _sim.OnConnect(HandleOnConnect);
        _sim.OnDisconnect(HandleOnDisconnect);
        _sim.OnTick(HandleOnTick);
    }

    private void HandleOnInit(string seed)
    {
        Debug.Log("Sim client has initialized " + seed);
        SimFixedRateManager.NetworkInitialized = true;
    }

    private void HandleOnTick(long tick)
    {
        // if (tick > 100 && tick % 100 == 0)
        // {
        //     // send a hash check message...
        //     SendMessage(new HashCheckMessage(Log.GetHashForTick(tick - 100), tick - 100));
        // }

        SimFixedRateManager.AllowTick(tick);
        Log.AddMessage(tick, new TickMessage(tick));
    }

    private void HandleOnConnect(string dbid)
    {
        Debug.Log("Sim client has connection from " + dbid);

        // listen for messages from this player...
        var dbidNumber = long.Parse(dbid);

        ListenForMessageFrom<PlayerSpawnCubeMessage>(dbid);
        ListenForMessageFrom<PlayerDestroyAllMessage>(dbid);
        // ListenForMessageFrom<HashCheckMessage>(dbid);
        _sim.On<HashCheckMessage>(nameof(HashCheckMessage), dbid, hashCheck =>
        {
            hashCheck.FromPlayer = dbidNumber;
            if (dbidNumber == LocalDbid) return;
            Debug.Log("Validating hash from " + dbid + " for tick " + hashCheck.ForTick);
            Log.AssertHashMatches(hashCheck.ForTick, hashCheck.Hash);

        });


        // _sim.On<PlayerSpawnCubeMessage>(dbid, message =>
        // {
        //     Log.AddMessage(message);
        // });
        // _sim.On()

        var joinMsg = new PlayerJoinedMessage
        {
            Tick = SimFixedRateManager.HighestSeenNetworkTick,
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

    public void SendMessage(Message message)
    {
        message.Tick = SimFixedRateManager.HighestSeenNetworkTick + 1; // this message belongs on the next tick...
        Debug.Log("Sending message " + message.Tick);
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
            // callback(message);
        });
    }

}
