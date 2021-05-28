using System;

namespace Simulation
{
   [Serializable]
   public abstract class Message
   {
      public long Tick;
      public long FromPlayer;
      // public bool IsWalking;
      // public uint DirectionX;
      // public uint DirectionY;

      public bool Available { get; private set; } = true;
      public void Consume()
      {
         Available = false;
      }
   }

   public class PlayerSpawnCubeMessage : Message
   {

   }

   public class PlayerDestroyAllMessage : Message
   {

   }

   public class HashCheckMessage : Message
   {
      public long ForTick;
      public string Hash;
      public HashCheckMessage(string hash, long forTick)
      {
         Hash = hash;
         ForTick = forTick;
      }
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