using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Simulation
{

     public class MyFixedRateCatchUpManager : IFixedRateManager
        {
            // TODO: move this to World
            float m_MaximumDeltaTime;
            public float MaximumDeltaTime
            {
                get => m_MaximumDeltaTime;
                set => m_MaximumDeltaTime = math.max(value, m_FixedTimestep);
            }

            float m_FixedTimestep;
            public float Timestep
            {
                get => m_FixedTimestep;
                set
                {
                    m_FixedTimestep = math.clamp(value, .0001f, 100000);
                }
            }

            double m_LastFixedUpdateTime;
            long m_FixedUpdateCount;
            bool m_DidPushTime;
            double m_MaxFinalElapsedTime;

            public MyFixedRateCatchUpManager(float fixedDeltaTime)
            {
                Timestep = fixedDeltaTime;
            }

            public bool ShouldGroupUpdate(ComponentSystemGroup group)
            {
                float worldMaximumDeltaTime = group.World.MaximumDeltaTime;
                float maximumDeltaTime = math.max(worldMaximumDeltaTime, m_FixedTimestep);

                // if this is true, means we're being called a second or later time in a loop
                if (m_DidPushTime)
                {
                    group.World.PopTime();
                }
                else
                {
                    m_MaxFinalElapsedTime = m_LastFixedUpdateTime + maximumDeltaTime;
                }

                var finalElapsedTime = math.min(m_MaxFinalElapsedTime, group.World.Time.ElapsedTime);
                if (m_FixedUpdateCount == 0)
                {
                    // First update should always occur at t=0
                }
                else if (finalElapsedTime - m_LastFixedUpdateTime >= m_FixedTimestep)
                {
                    // Advance the timestep and update the system group
                    m_LastFixedUpdateTime += m_FixedTimestep;
                }
                else
                {
                    // No update is necessary at this time.
                    m_DidPushTime = false;
                    return false;
                }

                m_FixedUpdateCount++;

                group.World.PushTime(new TimeData(
                    elapsedTime: m_LastFixedUpdateTime,
                    deltaTime: m_FixedTimestep));

                m_DidPushTime = true;
                return true;
            }
        }

   public class SimFixedRateManager : IFixedRateManager
   {
      private static int SimCalls;

      public static int NetworkFramesPerSecond = 20; // TODO: Keep in sync with Unity setting...

      public static long HighestSeenNetworkTick;
      public static bool Mask;

      public static bool NetworkInitialized;
      private float elapsed;

      public static double ElapsedPhysicsTime;
      public static long PhysicsTick, PhysicsTickLowerBound, PhysicsTickUpperBound;
      public static double last;

      public static bool FirstCheckSinceAllow;
      private FixedRateUtils.FixedRateSimpleManager _simpleManager;

      public SimFixedRateManager(float fixedDeltaTime)
      {
         Timestep = fixedDeltaTime;
         _simpleManager = new FixedRateUtils.FixedRateSimpleManager(fixedDeltaTime);
      }

      public static void AllowTick(long tick)
      {
         // we've seen this tick, and we know what the tick frame rate is, so we can calculate a
         HighestSeenNetworkTick = tick;
         Mask = true;
         FirstCheckSinceAllow = true;


      }

      public bool ShouldGroupUpdate(ComponentSystemGroup group)
      {
         // return _simpleManager.ShouldGroupUpdate(group);

         if (!NetworkInitialized)
         {
            group.World.PushTime(new TimeData(0, 0));
            return false;
         }

         var time = group.World.Time.ElapsedTime;
         var fpsPhysics = Timestep;
         var fpsNetwork = 1f / NetworkFramesPerSecond;
         var fpsConversion = (fpsNetwork / fpsPhysics); // should always equal 3...
         var futureTolerance = 1;
         var pastTolerance = 20;

         var tickPhysics = (long) (time / fpsPhysics);
         var tickNetwork = HighestSeenNetworkTick;
         PhysicsTick = tickPhysics;
         ElapsedPhysicsTime = time;


         var upperTickBoundPhysics = (long) (fpsConversion * (tickNetwork + futureTolerance));
         var lowerTickBoundPhysics = (long) (fpsConversion * (tickNetwork - pastTolerance));

         PhysicsTickLowerBound = lowerTickBoundPhysics;
         PhysicsTickUpperBound = upperTickBoundPhysics;

         var isPhysicsTooOld = tickPhysics <= lowerTickBoundPhysics;
         var isPhysicsTooNew = tickPhysics >= upperTickBoundPhysics;

         if (isPhysicsTooNew)
         {
            // stop the simulation.
            return false;
         }
         var nextElapsedTime = time + Timestep;
         if (isPhysicsTooOld)
         {
            // fast forward the simulation
            group.World.PushTime(new TimeData(nextElapsedTime, Timestep));
            return true;
         }

         // play the simulation at a regular pace.
         return _simpleManager.ShouldGroupUpdate(group);

         var m = Mask;
         if (m)
         {
            Mask = !Mask;

            // simulate up to the point where we've done a fixed timestep...

            var deltaTime = Time.deltaTime < Timestep ? Time.deltaTime : Timestep;// math.min(UnityEngine.Time.deltaTime, Timestep);
            group.World.PushTime(new TimeData(time + deltaTime, deltaTime));
         }

         return m;
      }

      public float Timestep { get; set; }
   }

}