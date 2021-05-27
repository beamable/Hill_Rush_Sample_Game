using System;

namespace Simulation
{
   [Serializable]
   public class Message
   {
      public long Tick;
      public long FromPlayer;
      public bool IsWalking;
      public uint DirectionX;
      public uint DirectionY;
   }

   public class PlayerJoinedMessage : Message
   {
      public PlayerJoinedMessage()
      {

      }
   }

   public class TickMessage : Message
   {
      public long Tick;
      public TickMessage(long tick)
      {
         Tick = tick;
      }
   }
}