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

      private Dictionary<long, string> _pendingHashValidations = new Dictionary<long, string>();

      private long latestInvalidFrame = -1;
      private long latestValidFrame = -1;

      public bool HasHashForTick(long tick)
      {
         return _tickToHash.ContainsKey(tick);
      }

      public bool TryGetInvalidHashTick(out long tick)
      {
         tick = latestInvalidFrame;
         return tick > -1;
      }

      public bool TryGetValidHashTick(out long tick)
      {
         tick = latestValidFrame;
         return tick > -1;
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
         if (_pendingHashValidations.TryGetValue(tick, out var pendingAssertHash))
         {
            AssertHash(tick, pendingAssertHash);
            _pendingHashValidations.Remove(tick);
         }
      }

      public void EnqueueHashAssertion(long tick, string hash)
      {
         if (!HasHashForTick(tick))
         {
            if (_pendingHashValidations.ContainsKey(tick))
            {
               throw new Exception("There is already a pending hash validation for tick " + tick);
            }
            _pendingHashValidations.Add(tick, hash);
            return;
         }

         AssertHash(tick, hash);
      }

      public bool AssertHash(long tick, string hash)
      {
         var actualHash = _tickToHash[tick];
         if (!Equals(actualHash, hash))
         {
            Debug.LogWarning("HASH MISMATCH!!! FOR TICK " + tick);
            latestInvalidFrame = tick;
            return false;
         }
         else
         {
            Debug.Log("Hash pass for tick: " + tick);
            latestValidFrame = tick;
            return true;
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

      public void NotifyConsumers(float elapsedTime, float deltaTime)
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
         var tick = NetworkController.HighestSeenNetworkFrame;
         var seenTick = _consumerIdToTick[consumerId];
         var timestep = 1f / NetworkController.NetworkFramesPerSecond;

         for (var t = seenTick + 1; t <= tick; t++)
         {
            var elapsedTime = t / (float)NetworkController.NetworkFramesPerSecond;

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