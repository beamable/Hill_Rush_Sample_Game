using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Api;
using Beamable.Experimental.Api.Sim;
using Beamable.Service;
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
        _sim = new SimClient(new MySimNetworkEventStream(roomId), NetworkFramesPerSecond, 1);
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

    private void Update()
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


     public class MySimNetworkEventStream : SimNetworkInterface {
      private static long REQ_FREQ_MS = 10;

      public string ClientId { get; private set; }
      public bool Ready { get; private set; }
      private List<SimEvent> _eventQueue = new List<SimEvent>();
      private List<SimFrame> _syncFrames = new List<SimFrame>();
      private List<SimFrame> _emptyFrames = new List<SimFrame>();
      private SimFrame _nextFrame = new SimFrame(-1, new List<SimEvent>());
      private long _lastReqTime;
      private bool hasData = false;
      private string roomName;

      public MySimNetworkEventStream(string roomName)
      {
         this.roomName = roomName;
         ClientId = ServiceManager.Resolve<PlatformService>().User.id.ToString();
         _syncFrames.Add(_nextFrame);
         Ready = true;
      }

      public List<SimFrame> Tick (long curFrame, long maxFrame, long expectedMaxFrame)
      {
         long now = GetTimeMs();
         if ((now -_lastReqTime) >= REQ_FREQ_MS)
         {
            _lastReqTime = now;
            var platform = ServiceManager.Resolve<PlatformService>();
            var req = new GameRelaySyncMsg();
            req.t = _nextFrame.Frame;
            for (int i=0; i < _eventQueue.Count; i++) {
               var evt = new GameRelayEvent();
               evt.FromSimEvent(_eventQueue[i]);
               req.events.Add(evt);
            }
            _eventQueue.Clear();

            platform.GameRelay.Sync(roomName, req).Then(rsp =>
            {
               if (rsp.t == -1)
               {
                  return;
               }

               // Purge all events we already might know about
               for (int j=0; j<rsp.events.Count; j++) {
                  var evt = rsp.events[j];
                  if (evt.t > _nextFrame.Frame) {
                     _nextFrame.Events.Add(evt.ToSimEvent());
                  }
               }

               // If the response is higher than what we know, let's get it
               if (rsp.t > _nextFrame.Frame) {
                  _nextFrame.Frame = rsp.t;
                  hasData = true;
               }
            });
         }
         if (hasData)
         {
            hasData = false;
            return _syncFrames;
         }

         _nextFrame.Events.Clear();
         return _emptyFrames;
      }

      private long GetTimeMs () {
         return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
      }

      public void SendEvent (SimEvent evt) {
         _eventQueue.Add(evt);
      }
   }
}
