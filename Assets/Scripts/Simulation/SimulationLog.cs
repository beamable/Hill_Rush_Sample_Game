using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation
{
   public class SimulationLog
   {
      private Dictionary<long, List<Message>> _tickToMessages = new Dictionary<long, List<Message>>();
      private long _highestTick = 0;
      private int _nextConsumerId;
      private Dictionary<int, Action<TimeUpdate>> _consumerIdToUpdater = new Dictionary<int, Action<TimeUpdate>>();

      private Dictionary<int, long> _consumerIdToTick = new Dictionary<int, long>();
      private Dictionary<long, string> _tickToHash = new Dictionary<long, string>();

      public bool HasHashForTick(long tick)
      {
         return _tickToHash.ContainsKey(tick);
      }

      public string GetHashForTick(long tick)
      {
         if (!HasHashForTick(tick))
         {
            throw new Exception("No hash has been calculated for tick " + tick);

         }

         return _tickToHash[tick];
      }

      public void ReportHashForTick(long tick, string hash)
      {
         _tickToHash[tick] = hash;
      }

      public void AssertHashMatches(long tick, string testHash)
      {
         if (!HasHashForTick(tick))
         {
            throw new Exception("No hash has been calculated for this tick");
         }

         var actualHash = _tickToHash[tick];
         if (!Equals(actualHash, testHash))
         {
            Debug.LogWarning("HASH MISMATCH!!! FOR TICK " + tick);
            // throw new Exception("Hash mismatch for tick " + tick);
         }
         else
         {
            Debug.Log("Hash pass for tick: " + tick);
         }
      }

      public IEnumerable<Message> GetMessagesForTick(long dbid, long tick)
      {
         return GetMessagesForTick(tick).Where(message => message.FromPlayer == dbid);
      }

      public IEnumerable<T> GetMessagesForTick<T>(long dbid, long tick)
         where T : Message
      {
         return GetMessagesForTick(tick).Where(message => message.FromPlayer == dbid && message is T).Cast<T>();
      }

      public IEnumerable<T> GetMessagesForTick<T>(long tick)
         where T : Message
      {
         return GetMessagesForTick(tick).Where(message => message is T).Cast<T>();
      }

      public int CreateNewConsumer(Action<TimeUpdate> onUpdate, long startTick=-1)
      {
         _nextConsumerId++;
         _consumerIdToTick.Add(_nextConsumerId, startTick);
         _consumerIdToUpdater.Add(_nextConsumerId, onUpdate);
         return _nextConsumerId;
      }

      public void NotifyUpdates(float elapsedTime, float deltaTime)
      {
         var update = new TimeUpdate
         {
            ElapsedTime = elapsedTime,
            DeltaTime = deltaTime
         };
         foreach (var updater in _consumerIdToUpdater.Values)
         {
            updater(update);
         }
      }

      public IEnumerable<Message> GetMessagesForTick(long tick)
      {
         if (!_tickToMessages.ContainsKey(tick))
         {
            yield break;
         }

         var messages = _tickToMessages[tick];
         foreach (var message in messages)
         {
            if (!message.Available) continue;
            yield return message;
         }
      }

      public IEnumerable<Message> GetMessagesForTick(long tick, int consumerId)
      {
         if (!_consumerIdToTick.TryGetValue(consumerId, out var lastTick))
         {
            lastTick = -1;
         }
         if (tick > lastTick)
         {
            _consumerIdToTick[consumerId] = tick;
            foreach (var message in GetMessagesForTick(tick))
            {
               yield return message;
            }
         }
      }

      public IEnumerable<TimeUpdate> GetTimeUpdates(int consumerId)
      {
         var tick = SimFixedRateManager.HighestSeenNetworkTick;
         var seenTick = _consumerIdToTick[consumerId];
         var timestep = 1f / SimFixedRateManager.NetworkFramesPerSecond;

         for (var t = seenTick + 1; t <= tick; t++)
         {
            var elapsedTime = t / (float)SimFixedRateManager.NetworkFramesPerSecond;

            var messages = GetMessagesForTick(t, consumerId).ToList();
            yield return new TimeUpdate
            {
               ElapsedTime = elapsedTime,
               DeltaTime = timestep,
               Messages = messages
            };
         }
      }

      public void AddMessage(Message message)
      {
         Debug.Log("Adding " + message.GetType().Name + " at " + _highestTick + " from " + message.FromPlayer);
         AddMessage(_highestTick, message);
      }

      public void AddMessage(long tick, Message message)
      {
         _highestTick = tick > _highestTick
            ? tick
            : _highestTick;
         if (!_tickToMessages.ContainsKey(tick))
         {
            _tickToMessages.Add(tick, new List<Message>());
         }

         var messages = _tickToMessages[tick];
         messages.Add(message);
      }
   }

   public class TimeUpdate
   {
      public float ElapsedTime;
      public float DeltaTime;
      public List<Message> Messages;
   }
}