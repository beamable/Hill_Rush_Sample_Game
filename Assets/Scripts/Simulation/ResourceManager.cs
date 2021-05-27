using UnityEngine;

namespace Simulation
{
   public class ResourceManager : MonoBehaviour
   {
      public Material defaultMaterial;
      public MeshRenderer CubePrefab;

      public Material[] colorMaterials;

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

      private void Awake()
      {
         _instance = this;
         materialPropertyBlock = new MaterialPropertyBlock();
      }
   }

}