using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityS.Mathematics;
using UnityS.Physics.Extensions;

namespace Simulation
{
   public class GameSimulation : MonoBehaviour
   {
      public NetworkController NetworkController;

      public PlayerBehaviour PlayerPrefab;

      private Dictionary<long, (PlayerBehaviour, Entity)> _dbidToPlayer = new Dictionary<long,  (PlayerBehaviour, Entity)>();
      private Queue<Message> _messages;

      public void Start()
      {
         _messages = NetworkController.Init();
         StartCoroutine(ProcessMessages(_messages));
      }

      private void FixedUpdate()
      {

      }
      //
      // private void Update()
      // {
      //    if (_messages.Count == 0)
      //    {
      //       return;
      //    }
      //
      //    var message = _messages.Dequeue();
      //
      //    // do something....?
      //    switch (message)
      //    {
      //       case TickMessage tickMessage:
      //          SimFixedRateManager.AllowTick(tickMessage.Tick);
      //          break;
      //       case PlayerJoinedMessage joinedMessage:
      //          // spawn a physics object for the player...
      //          if (_dbidToPlayer.ContainsKey(joinedMessage.FromPlayer))
      //          {
      //             break; // don't duplicate player
      //          }
      //
      //          Debug.Log("A player has joined!");
      //
      //          UnityS.Physics.Material material = UnityS.Physics.Material.Default;
      //          material.Friction = (sfloat) 0.05f;
      //
      //          PhysicsParams physicsParams = PhysicsParams.Default;
      //          physicsParams.isDynamic = true;
      //          physicsParams.startingLinearVelocity = float3.zero;
      //          physicsParams.mass = (sfloat) .25f;
      //
      //
      //          var playerData = GameController.Instance.CreateBoxColliderObject(PlayerPrefab,
      //             new float3((sfloat) 0, (sfloat) 10, (sfloat) 0),
      //             new float3((sfloat) 1f, (sfloat) 1f, (sfloat) 1f), quaternion.identity, material, physicsParams);
      //
      //          var player = playerData.Item1;
      //          player.NetworkController = NetworkController; // TODO
      //          _dbidToPlayer.Add(joinedMessage.FromPlayer, playerData);
      //          break;
      //       default:
      //          // track the player's state...
      //          var plr = _dbidToPlayer[message.FromPlayer];
      //          // Debug.Log("PLR : " + message.FromPlayer + "  " + message.State.IsWalking);
      //          SharedInputData.SetMove(plr.Item2, new MoveForceData
      //          {
      //             Magnitude = (sfloat) (message.IsWalking ? 100 : 0),
      //             Direction = new float2(sfloat.FromRaw(message.DirectionX), sfloat.FromRaw(message.DirectionY)),
      //             X = (message.IsWalking ? 1000 : 0)
      //          });
      //          break;
      //    }
      //
      // }

      IEnumerator ProcessMessages(Queue<Message> messages)
      {

         while (true)
         {
            if (messages.Count == 0)
            {
               // there are no messages to process; so let the simulation run...
               yield return new WaitForEndOfFrame();
               continue;
            }

            var message = messages.Dequeue();
            // do something....?
            switch (message)
            {
               case TickMessage tickMessage:
                  SimFixedRateManager.AllowTick(tickMessage.Tick);
                  break;
               case PlayerJoinedMessage joinedMessage:
                  // spawn a physics object for the player...
                  if (_dbidToPlayer.ContainsKey(joinedMessage.FromPlayer))
                  {
                     break; // don't duplicate player
                  }
                  Debug.Log("A player has joined!");

                  UnityS.Physics.Material material = UnityS.Physics.Material.Default;
                  material.Friction = (sfloat)0.05f;

                  PhysicsParams physicsParams = PhysicsParams.Default;
                  physicsParams.isDynamic = true;
                  physicsParams.startingLinearVelocity = float3.zero;
                  physicsParams.mass = (sfloat).25f;


                  var playerData = GameController.Instance.CreateBoxColliderObject(PlayerPrefab, new float3((sfloat)0, (sfloat)10, (sfloat)0),
                     new float3((sfloat)1f, (sfloat)1f, (sfloat)1f), quaternion.identity, material, physicsParams);

                  var player = playerData.Item1;
                  player.NetworkController = NetworkController; // TODO
                  _dbidToPlayer.Add(joinedMessage.FromPlayer, playerData);
                  break;
               default:
                  // track the player's state...
                  var plr = _dbidToPlayer[message.FromPlayer];
                  Debug.Log("Rec [" + message.Tick + "]: " + message.FromPlayer + "  " + message.IsWalking + "  " + sfloat.FromRaw(message.DirectionX) + " | " + sfloat.FromRaw(message.DirectionY));
                  SharedInputData.SetMove(plr.Item2, new MoveForceData
                  {
                     Magnitude = (sfloat) (message.IsWalking ? 100 : 0),
                     Direction = new float2(sfloat.FromRaw(message.DirectionX), sfloat.FromRaw(message.DirectionY)),
                     X = (message.IsWalking ? 1000 : 0)
                  });
                  break;
            }


            // yield return new WaitForEndOfFrame();
         }
      }
   }
}