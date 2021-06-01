using System;

namespace Simulation
{
   [Serializable]
   public abstract class Message
   {
      public long Tick; // TODO: Remove this. We don't _need_ it.
      public long FromPlayer;

      public bool Available { get; private set; } = true; // TODO: refactor "availability" into the sim log as a global consumer
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

   public class PlayerInputMessage : Message
   {
      public uint XDirection;
      public uint YDirection;
      public uint Speed;

      /// <summary>
      /// The amount of time AFTER the start of the network frame, to the point when the input was started. f
      /// </summary>
      public uint ForcedLagTime;

      public float Time;

      public uint StartWorldTime;

      public PlayerInputMessage(){}
      public PlayerInputMessage(uint xDirection, uint yDirection, uint speed)
      {
         XDirection = xDirection;
         YDirection = yDirection;
         Speed = speed;
      }
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
      public new long Tick;
      public TickMessage(long tick)
      {
         Tick = tick;
      }
   }
}