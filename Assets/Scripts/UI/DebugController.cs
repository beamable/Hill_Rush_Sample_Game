using System;
using Simulation;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BeamableExtensions.UI
{
   public class DebugController : MonoBehaviour
   {
      public TextMeshProUGUI TickText, ElapsedTimeText, PhysicsTickText, PhysTickLower, PhysTickUpper;

      public NetworkController NetworkController;

      public Button QuitButton;

      private void Start()
      {
         QuitButton.onClick.AddListener(HandleQuit);
      }

      private void Update()
      {
         TickText.text = "SIM_TICK: " + SimFixedRateManager.HighestSeenNetworkTick.ToString();
         PhysicsTickText.text = "PHYS_TICK: " + SimFixedRateManager.PhysicsTick.ToString();
         ElapsedTimeText.text = "PHYS_TIME: " + SimFixedRateManager.ElapsedPhysicsTime.ToString();
         PhysTickLower.text = "PHYS_TICK_LOWER: " + SimFixedRateManager.PhysicsTickLowerBound.ToString();
         PhysTickUpper.text = "PHYS_TICK_UPPER: " + SimFixedRateManager.PhysicsTickUpperBound.ToString();
      }

      void HandleQuit()
      {
         // stop all systems...
         var world = World.DefaultGameObjectInjectionWorld;
         var timeSystem = world.GetOrCreateSystem<SimWorldsTimeSystem>();
         var gameController = world.GetOrCreateSystem<GameController>();
         var gameSystem = world.GetOrCreateSystem<GameSystem>();
         var inputSystem = world.GetOrCreateSystem<InputSystem>();
         var hashingSystem = world.GetOrCreateSystem<HashingSystem>();

         var initGroup = world.GetExistingSystem<InitializationSystemGroup>();
         var fixedGroup = world.GetExistingSystem<FixedStepSimulationSystemGroup>();

         world.DestroySystem(timeSystem);
         world.DestroySystem(gameController);
         world.DestroySystem(gameSystem);
         world.DestroySystem(inputSystem);
         world.DestroySystem(hashingSystem);

         SimFixedRateManager.HighestSeenNetworkTick = 0;
         SimFixedRateManager.NetworkInitialized = false;

         fixedGroup.RemoveSystemFromUpdateList(gameController);
         initGroup.RemoveSystemFromUpdateList(timeSystem);
         fixedGroup.RemoveSystemFromUpdateList(gameSystem);
         fixedGroup.RemoveSystemFromUpdateList(inputSystem);
         fixedGroup.RemoveSystemFromUpdateList(hashingSystem);

         SceneManager.LoadScene("DebugJoin");
      }
   }
}