using System;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Simulation
{
   public class ResourceManager : MonoBehaviour
   {
      public Material defaultMaterial;
      public MeshRenderer CubePrefab;
      public PlayerBehaviour PlayerPrefab;

      public Material[] colorMaterials;

      public NetworkController NetworkController;

      public Material RealRandomMaterial => colorMaterials[Random.Range(0, colorMaterials.Length)];

      public int seed;

      private static ResourceManager _instance;
      public static ResourceManager Instance
      {
         get
         {
            if (_instance == null)
            {
               _instance = FindObjectOfType<ResourceManager>();
            }

            return _instance;
         }
      }

      public static MaterialPropertyBlock materialPropertyBlock;

      private void OnDestroy()
      {
         _instance = null;
      }

      private void Awake()
      {
         _instance = this;
         materialPropertyBlock = new MaterialPropertyBlock();


         // create systems for game...
         var world = World.DefaultGameObjectInjectionWorld;
         var timeSystem = world.GetOrCreateSystem<SimWorldsTimeSystem>();
         var gameController = world.GetOrCreateSystem<GameController>();
         var gameSystem = world.GetOrCreateSystem<GameSystem>();
         var inputSystem = world.GetOrCreateSystem<InputSystem>();

         var oldTime = world.GetExistingSystem<UpdateWorldTimeSystem>();
         if (oldTime != null)
         {
            world.DestroySystem(oldTime); // don't allow time to ever move forward.
         }

         var initGroup = world.GetExistingSystem<InitializationSystemGroup>();
         var fixedGroup = world.GetExistingSystem<FixedStepSimulationSystemGroup>();

         fixedGroup.AddSystemToUpdateList(gameController);
         initGroup.AddSystemToUpdateList(timeSystem);
         initGroup.RemoveSystemFromUpdateList(oldTime);
         fixedGroup.AddSystemToUpdateList(gameSystem);
         fixedGroup.AddSystemToUpdateList(inputSystem);

         //world.AddSystem(gameController);


         initGroup.SortSystems();
         fixedGroup.SortSystems();

         // ScriptBehaviourUpdateOrder.Add
         // ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
      }
   }

}