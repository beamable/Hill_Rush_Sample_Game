using System;
using Simulation;
using TMPro;
using UnityEngine;

namespace BeamableExtensions.UI
{
   public class DebugController : MonoBehaviour
   {
      public TextMeshProUGUI TickText, ElapsedTimeText, PhysicsTickText, PhysTickLower, PhysTickUpper;

      public NetworkController NetworkController;

      private void Update()
      {
         TickText.text = "SIM_TICK: " + SimFixedRateManager.HighestSeenNetworkTick.ToString();
         PhysicsTickText.text = "PHYS_TICK: " + SimFixedRateManager.PhysicsTick.ToString();
         ElapsedTimeText.text = "PHYS_TIME: " + SimFixedRateManager.ElapsedPhysicsTime.ToString();
         PhysTickLower.text = "PHYS_TICK_LOWER: " + SimFixedRateManager.PhysicsTickLowerBound.ToString();
         PhysTickUpper.text = "PHYS_TICK_UPPER: " + SimFixedRateManager.PhysicsTickUpperBound.ToString();
      }
   }
}