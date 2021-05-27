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

      public bool needsToSendInput;
      public long lastSeenTick;

      void Start()
      {
         PlayerInput.OnInputChanged += input =>
         {
            needsToSendInput = true;

         };
         // StartCoroutine(ConstantlyReportInput());
         // NetworkController.SimClient.OnTick(tickNumber =>
         // {
         //    // broadcast message about player state...
         //    NetworkController.SendMessage(new Message
         //    {
         //       // Tick = tickNumber,
         //       IsWalking = _nextState.IsWalking,
         //       DirectionX = _nextState.Direction.x.RawValue,
         //       DirectionY = _nextState.Direction.y.RawValue,
         //    });
         // });
      }

      private void Update()
      {
         var currTick = SimFixedRateManager.HighestSeenNetworkTick;
         if (currTick != lastSeenTick)
         {
            if (needsToSendInput)
            {
               needsToSendInput = false;
               var input = PlayerInput;
               Debug.Log("Sending Player Input Change " + input.IsWalking + " " + input.Direction.x + " | " + input.Direction.y);
               NetworkController.SendMessage(new Message
               {
                  IsWalking = input.IsWalking,
                  DirectionX = ((sfloat)input.Direction.x).RawValue,
                  DirectionY = ((sfloat)input.Direction.y).RawValue,
               });
            }
         }

         lastSeenTick = currTick;
      }
   }
}