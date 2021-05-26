using System;
using UnityEngine;
using UnityS.Mathematics;

namespace Simulation
{
   [Serializable]
   public class PlayerFrameState
   {
      public float2 Direction;
      public bool IsWalking;
   }

   public class PlayerInput : MonoBehaviour
   {
      public float2 Direction;
      public bool IsWalking;

      private void Update()
      {
         var dir = new Vector2();
         if (Input.GetKey(KeyCode.UpArrow))
         {
            dir.y += 1;
         }

         if (Input.GetKey(KeyCode.DownArrow))
         {
            dir.y -= 1;
         }

         if (Input.GetKey(KeyCode.RightArrow))
         {
            dir.x += 1;
         }

         if (Input.GetKey(KeyCode.LeftArrow))
         {
            dir.x -= 1;
         }

         IsWalking = dir.magnitude > .1f;

         Direction = new float2((sfloat) dir.x, (sfloat) dir.y);
      }
   }
}