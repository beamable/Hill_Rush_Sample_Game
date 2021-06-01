using System;
using Unity.Entities;
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

         if (Input.GetKeyDown(KeyCode.A))
         {
            // at this moment, the player has request a move.
            // var simTime = NetworkController.SimTime;

            // get the current time.
            var time = (sfloat)World.DefaultGameObjectInjectionWorld.Time.ElapsedTime;

            // snap it to the next high-resolution tick rate. (1/60f)
            var fixedTickRate = sfloat.One / (sfloat) 60;
            var startFixedTick = math.floor(time / fixedTickRate);
            var nextFixedTick = startFixedTick + sfloat.One;
            var nextFixedTickTime = nextFixedTick * fixedTickRate;

            var networkTickRate = sfloat.One / (sfloat) NetworkController.NetworkFramesPerSecond;
            var startNetworkTick = math.floor(time / networkTickRate);
            var startNetworkTickTime = startNetworkTick * networkTickRate;

            var forcedLagTime = nextFixedTickTime - startNetworkTickTime;

            Debug.Log("sending at " + time);
            ResourceManager.Instance.NetworkController.SendNetworkMessage(new PlayerInputMessage
            {
               Speed = 1,
               XDirection = 2,
               YDirection = 3,
               StartWorldTime = time.RawValue,
               Time = Time.realtimeSinceStartup,
               ForcedLagTime = forcedLagTime.RawValue
            });

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