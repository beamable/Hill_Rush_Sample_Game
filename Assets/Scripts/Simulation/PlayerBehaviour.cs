using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityS.Mathematics;

namespace Simulation
{
   public class PlayerBehaviour : MonoBehaviour
   {
      public PlayerInput PlayerInput;

      public float angle;

      public float rotationSpeed = 1;

      public float rotationRadius = 4;

      [Header("Internal Data")]
      [ReadOnly]
      public bool IsLocalPlayer = false;

      [ReadOnly]
      public long Dbid;

      [ReadOnly]
      public bool needsToSendInput;

      [ReadOnly]
      public long lastSeenTick;

      [ReadOnly]
      public int consumerId;

      void Start()
      {
         PlayerInput.OnInputChanged += input =>
         {
            needsToSendInput = true;
         };
      }

      public void Setup(long dbid)
      {
         Dbid = dbid;
         IsLocalPlayer = ResourceManager.Instance.NetworkController.LocalDbid == dbid;
         consumerId = ResourceManager.Instance.NetworkController.Log.CreateNewConsumer(NetworkUpdate);


         if (IsLocalPlayer)
         {
            ResourceManager.Instance.ShootButton.onClick.AddListener(() =>
            {
               needsToSendInput = true;
               PlayerInput.SpawnRequest = true;
            });
            ResourceManager.Instance.ClearButton.onClick.AddListener(() =>
            {
               needsToSendInput = true;
               PlayerInput.DeleteRequested = true;
            });
         }
      }

      void NetworkUpdate(TimeUpdate update)
      {
         angle += rotationSpeed * update.DeltaTime;
         transform.position = new Vector3(rotationRadius * Mathf.Cos(angle), 4, rotationRadius * Mathf.Sin(angle));

      }

      private void Update()
      {

         // rotate around in a circle
         // TODO: Put this in a network time loop...
         //
         // var network = ResourceManager.Instance.NetworkController;
         // foreach (var t in network.Log.GetTimeUpdates(consumerId))
         // {



         var currTick = SimFixedRateManager.HighestSeenNetworkTick;
         if (currTick != lastSeenTick)
         {
            if (IsLocalPlayer && needsToSendInput)
            {
               needsToSendInput = false;
               if (PlayerInput.DeleteRequested)
               {
                  ResourceManager.Instance.NetworkController.SendMessage(new PlayerDestroyAllMessage());
                  PlayerInput.DeleteRequested = false;
               } else if (PlayerInput.SpawnRequest)
               {
                  ResourceManager.Instance.NetworkController.SendMessage(new PlayerSpawnCubeMessage());
                  PlayerInput.SpawnRequest = false;
               }

               // needsToSendInput = false;
               // var input = PlayerInput;
               // Debug.Log("Sending Player Input Change " + input.IsWalking + " " + input.Direction.x + " | " + input.Direction.y);
               // NetworkController.SendMessage(new Message
               // {
               //    IsWalking = input.IsWalking,
               //    DirectionX = ((sfloat)input.Direction.x).RawValue,
               //    DirectionY = ((sfloat)input.Direction.y).RawValue,
               // });
            }
         }

         lastSeenTick = currTick;
      }

      private List<(MeshRenderer, Entity)> _cubeEntities = new List<(MeshRenderer, Entity)>();

      public void ShootCube()
      {
         var position = new float3(
            (sfloat)transform.position.x,
            (sfloat)transform.position.y,
            (sfloat)transform.position.z
            );
         _cubeEntities.Add(GameController.Instance.SpawnCube(Dbid, position));
      }

      public void DestroyAllCubes()
      {
         Debug.Log("Destroying all player cubes...");
         foreach (var cube in _cubeEntities)
         {
            Destroy(cube.Item1.gameObject);
            GameController.Instance.DestroyEntity(cube.Item2);
         }
         _cubeEntities.Clear();
      }
   }
}