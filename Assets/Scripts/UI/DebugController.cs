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
      public TextMeshProUGUI TickText, ElapsedTimeText, HashErrorText;

      public NetworkController NetworkController;

      public Button QuitButton;

      private void Start()
      {
         QuitButton.onClick.AddListener(HandleQuit);
      }

      private void Update()
      {
         TickText.text = "SIM_TICK: " + NetworkController.HighestSeenNetworkFrame.ToString();
         ElapsedTimeText.text = "ELAPSED: " + World.DefaultGameObjectInjectionWorld.Time.ElapsedTime;
         HashErrorText.text = "HASH ERROR: NO";

         if (NetworkController.Log.TryGetInvalidHashTick(out var invalidTick))
         {
            // this is pretty much game over, because we don't support Rollback (yet)
            HashErrorText.color = Color.red;
            HashErrorText.text = "HASH ERROR: YES, AT TICK " + invalidTick;
         }
      }

      void HandleQuit()
      {
         // stop all systems...
         SystemManager.DestroyGameSystems();
         SceneManager.LoadScene("DebugJoin");
      }
   }
}