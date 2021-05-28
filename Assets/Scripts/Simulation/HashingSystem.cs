using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityS.Physics;

namespace Simulation
{
   [DisableAutoCreation]
   public class HashingSystem : SystemBase
   {
      protected override void OnUpdate()
      {
         Entities.ForEach((ref Entity e, ref Translation t, ref Rotation r, ref PhysicsMass mass, ref PhysicsVelocity _vel) =>
         {
            // _vel.ApplyLinearImpulse(mass, new float3((sfloat).01f, (sfloat)0, (sfloat)0));
            // update object transforms, based on ECS data
            // Debug.Log("UPDATE " + e.Index + " " + _vel.Linear.x);


         }).WithoutBurst().Run();
      }
   }
}