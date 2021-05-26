using System;
using System.Collections;
using UnityEngine;
using UnityS.Mathematics;

namespace Simulation
{
   public class PlayerBehaviour : MonoBehaviour
   {
      public PlayerInput PlayerInput;
      public NetworkController NetworkController;

      private PlayerFrameState _nextState = new PlayerFrameState();

      void Start()
      {
         StartCoroutine(ConstantlyReportInput());
         // NetworkController.SimClient.OnTick(tickNumber =>
         // {
         //    // broadcast message about player state...
         //    NetworkController.SendMessage(new Message
         //    {
         //       Tick = tickNumber,
         //       IsWalking = _nextState.IsWalking,
         //       DirectionX = _nextState.Direction.x.RawValue,
         //       DirectionY = _nextState.Direction.y.RawValue,
         //    });
         // });
      }

      public void Update()
      {
         _nextState.Direction = PlayerInput.Direction;
         _nextState.IsWalking = PlayerInput.IsWalking;


      }

      IEnumerator ConstantlyReportInput()
      {
         while (true)
         {
            yield return new WaitForSecondsRealtime(1f / NetworkController.framesPerSecond);
            // TODO: Schedule this on the "next tick" ?
            NetworkController.SendMessage(new Message
            {
               IsWalking = _nextState.IsWalking,
               DirectionX = _nextState.Direction.x.RawValue,
               DirectionY = _nextState.Direction.y.RawValue,
            });
         }
      }
   }
}