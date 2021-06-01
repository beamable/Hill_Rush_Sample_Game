using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
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

      public Button ShootButton, ClearButton;

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
         Debug.Log("RESOURCE MANAGER AWAKE AND INIT");
         _instance = this;
         materialPropertyBlock = new MaterialPropertyBlock();
         SystemManager.StartGameSystems();
      }
   }

}