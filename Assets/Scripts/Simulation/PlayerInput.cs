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
      public Vector2 Direction;
      public bool IsWalking;

      public event Action<PlayerInput> OnInputChanged;


      bool KeyCheck(KeyCode code, out bool isDown)
      {
         isDown = false;
         if (Input.GetKeyDown(code))
         {
            isDown = true;
            return true;
         }

         if (Input.GetKey(code))
         {
            isDown = true;
            return false;
         }

         if (Input.GetKeyUp(code))
         {
            return true;
         }

         return false;
      }

      private void Update()
      {
         var isDown = false;
         var isUp = false;
         var isRight = false;
         var isLeft = false;
         bool dirty = KeyCheck(KeyCode.UpArrow, out isUp)
                      || KeyCheck(KeyCode.DownArrow, out isDown)
                      || KeyCheck(KeyCode.RightArrow, out isRight)
                      || KeyCheck(KeyCode.LeftArrow, out isLeft);

         Direction.y = isUp ? 1 : (isDown ? -1 : 0);
         Direction.x = isRight ? 1 : (isLeft ? -1 : 0);

         IsWalking = Direction.magnitude > .1f;
         if (dirty)
         {
            OnInputChanged?.Invoke(this);
         }
      }
   }
}