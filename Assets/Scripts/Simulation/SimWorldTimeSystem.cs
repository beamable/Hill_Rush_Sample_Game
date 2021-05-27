using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;

namespace Simulation
{
   [Preserve]
   [UpdateInGroup(typeof(InitializationSystemGroup))]
   public class SimWorldTimeSystem : ComponentSystem
   {
      protected override void OnUpdate()
      {
         var now = World.Time.ElapsedTime;
         var fps = 1 / (float) SimFixedRateManager.NetworkFramesPerSecond;
         var simTime = SimFixedRateManager.HighestSeenNetworkTick * fps;
         var maxTime = simTime + fps * 1; // we can simulate the current network tick into the future.

         var useCatchUp = true;
         var minTime = simTime - fps * 5; // we can sit a bit in the past...

         var normalDelta = UnityEngine.Time.deltaTime;
         var normalNextTime = now + normalDelta;
         if (normalNextTime >= maxTime)
         {
            var fastDelta = maxTime - (float) now; // should equal zero, most of the time.
            // Debug.Log("TOO FAST: " + fastDelta);
            World.SetTime(new TimeData(maxTime, fastDelta ));
         }
         else if (useCatchUp && normalNextTime < minTime)
         {
            var catchUpTime = simTime - fps * 2;
            var slowDelta = catchUpTime - (float)now;
            // Debug.Log("TOO SLOW: " + normalNextTime + " < " + minTime + " || " + slowDelta);
            World.SetTime(new TimeData(catchUpTime,  slowDelta) );
         }
         else
         {
            World.SetTime(new TimeData(normalNextTime, normalDelta));
         }

      }
   }
}