using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using UnityS.Mathematics;

namespace Simulation
{
   [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
   [DisableAutoCreation]
   public class GameSystem : SystemBase
   {
      private Dictionary<long, PlayerBehaviour> _dbidToPlayer = new Dictionary<long,  PlayerBehaviour>();

      protected override void OnCreate()
      {
         base.OnCreate();
         var network = ResourceManager.Instance.NetworkController;
         var _ = network.Init();
      }

      protected override void OnUpdate()
      {
         // get the tick this represents...
         var network = ResourceManager.Instance.NetworkController;
         var time = World.Time.ElapsedTime;
         var frame = (long) (time * NetworkController.NetworkFramesPerSecond);

         var messages = network.Log.GetMessagesForTick(frame).ToList();

         network.Log.NotifyConsumers((float)time, (float)World.Time.DeltaTime);

         foreach (var message in messages)
         {
            switch (message)
            {
               case PlayerJoinedMessage join:
                  HandlePlayerJoin(join);
                  join.Consume();
                  break;
               case PlayerSpawnCubeMessage cube:
                  var shootPlr = _dbidToPlayer[message.FromPlayer];
                  shootPlr.ShootCube();
                  cube.Consume();
                  break;
               case PlayerDestroyAllMessage destroy:
                  var destroyPlr = _dbidToPlayer[message.FromPlayer];
                  destroyPlr.DestroyAllCubes();
                  destroy.Consume();
                  break;
               default:
                  break;
            }
         }


      }

      void HandlePlayerJoin(PlayerJoinedMessage joinedMessage)
      {
         if (_dbidToPlayer.ContainsKey(joinedMessage.FromPlayer))
         {
            return;
         }

         Debug.Log("A player has joined!");

         // UnityS.Physics.Material material = UnityS.Physics.Material.Default;
         // material.Friction = (sfloat)0.05f;
         //
         // PhysicsParams physicsParams = PhysicsParams.Default;
         // physicsParams.isDynamic = true;
         // physicsParams.startingLinearVelocity = float3.zero;
         // physicsParams.mass = (sfloat).25f;
         //
         //
         // var playerData = GameController.Instance.CreateBoxColliderObject(ResourceManager.Instance.PlayerPrefab, new float3((sfloat)0, (sfloat)10, (sfloat)0),
         //    new float3((sfloat)1f, (sfloat)1f, (sfloat)1f), quaternion.identity, material, physicsParams);

         var player = GameObject.Instantiate(ResourceManager.Instance.PlayerPrefab);
         player.Setup(joinedMessage.FromPlayer);
         _dbidToPlayer.Add(joinedMessage.FromPlayer, player);
      }
   }
}