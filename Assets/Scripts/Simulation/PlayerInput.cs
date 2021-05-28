using System;
using UnityEngine;
using UnityS.Mathematics;

namespace Simulation
{

   public class PlayerInput : MonoBehaviour
   {
      public bool SpawnRequest, DeleteRequested;

      public event Action<PlayerInput> OnInputChanged;

      private void Update()
      {
         if (Input.GetKeyDown(KeyCode.Space))
         {
            // spawn a cube
            SpawnRequest = true;
            OnInputChanged?.Invoke(this);
         }

         if (Input.GetKeyDown(KeyCode.D))
         {
            // spawn a cube
            DeleteRequested = true;
            OnInputChanged?.Invoke(this);
         }


         // var isDown = false;
         // var isUp = false;
         // var isRight = false;
         // var isLeft = false;
         // bool dirty = KeyCheck(KeyCode.UpArrow, out isUp)
         //              || KeyCheck(KeyCode.DownArrow, out isDown)
         //              || KeyCheck(KeyCode.RightArrow, out isRight)
         //              || KeyCheck(KeyCode.LeftArrow, out isLeft);
         //
         // Direction.y = isUp ? 1 : (isDown ? -1 : 0);
         // Direction.x = isRight ? 1 : (isLeft ? -1 : 0);
         //
         // IsWalking = Direction.magnitude > .1f;
         // if (dirty)
         // {
         //    OnInputChanged?.Invoke(this);
         // }
      }
   }
}