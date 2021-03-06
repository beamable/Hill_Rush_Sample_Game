using System.Collections.Generic;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using UnityS.Mathematics;
using UnityS.Physics;
using UnityS.Physics.Extensions;
using UnityS.Physics.Systems;

namespace Simulation
{
   public struct MoveForceData : IComponentData
   {
      public float2 Direction;
      public sfloat Magnitude;
      public float X;
   }

   public static class SharedInputData {
      public static readonly Dictionary<Entity, MoveForceData> _entityMoveTable = new Dictionary<Entity, MoveForceData>();

      public static void SetMove(Entity e, MoveForceData data)
      {
         _entityMoveTable[e] = data;
      }

   }


   [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
   [DisableAutoCreation]
   public class InputSystem : SystemBase
   {
      protected override void OnUpdate()
      {
         var timeDelta = (sfloat)World.Time.DeltaTime;

         // var moveTable = SharedInputData._entityMoveTable;
         Entities.WithoutBurst().ForEach((Entity e, ref PhysicsVelocity vel, ref PhysicsMass mass, ref MoveForceData move) =>
         {
            // if (moveTable.TryGetValue(e, out var nextMove))
            // {
            //    move.X = nextMove.X;
            //    move.Direction = nextMove.Direction;
            //    move.Magnitude = nextMove.Magnitude;
            // }

            var impulse = new float3(
               x: move.Direction.x,
               y: (sfloat) 0,
               z: move.Direction.y
            ) * move.Magnitude * timeDelta;


            vel.ApplyLinearImpulse(mass, impulse);

            // cap out the velocity at a given speed.
            var linearVelocityMag = math.length(vel.Linear);
            if (linearVelocityMag > (sfloat).01f)
            {
               var normalizedLinearVelocity = vel.Linear / linearVelocityMag;
               vel.Linear = normalizedLinearVelocity * math.min(linearVelocityMag, (sfloat)15);
            }

         }).Run();
      }
   }
}