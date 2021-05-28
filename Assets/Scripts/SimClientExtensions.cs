using System;
using System.Reflection;
using Beamable.Experimental.Api.Sim;
using Simulation;

namespace BeamableExtensions
{
   public static class SimClientExtensions
   {
      public static SimClient.EventCallback<string> On<T>(this SimClient client, string origin, SimClient.EventCallback<T> callback)
         where T : Message
      {
         var dbid = long.Parse(origin);
         return client.On<T>(typeof(T).Name, origin, message =>
         {
            message.FromPlayer = dbid;
            callback(message);
         });
      }
   }
}