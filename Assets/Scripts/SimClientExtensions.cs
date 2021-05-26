using System;
using System.Reflection;
using Beamable.Experimental.Api.Sim;

namespace BeamableExtensions
{
   public static class SimClientExtensions
   {
      public static SimClient.EventCallback<string> On<T>(this SimClient client, string origin, SimClient.EventCallback<T> callback)
      {
         return client.On<T>(typeof(T).ToString(), origin, callback);
      }

      public static void SendRaw<T>(this SimClient client, string evtName, T message, Func<T, string> preProcessor)
      {
         var raw = preProcessor(message);
         client.Network.SendEvent(new SimEvent(client.Network.ClientId, evtName, raw));
      }

      public static void OnRaw(this SimClient client, string evtName, string origin, Action<string> callback)
      {
         var method = client.GetType().GetMethod("OnInternal", BindingFlags.NonPublic | BindingFlags.Instance);
         method.Invoke(client, new object[] {evtName, origin, callback});

      }
   }
}