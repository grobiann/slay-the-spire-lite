using STSLite.Core.Multiplayer.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace STSLite.Core.Multiplayer
{
    public class NetMessageBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public string Serialize<T>(T message) where T : INetMessage
        {
            return $"{typeof(T).AssemblyQualifiedName}\n{SerializeObject(message)}";
        }

        public void Dispatch(string packetJson, ulong senderId)
        {
            int splitIndex = packetJson.IndexOf('\n');
            if (splitIndex < 0)
            {
                return;
            }

            string messageTypeName = packetJson.Substring(0, splitIndex);
            string payload = packetJson.Substring(splitIndex + 1);
            Type messageType = Type.GetType(messageTypeName);
            if (messageType == null || !typeof(INetMessage).IsAssignableFrom(messageType))
            {
                return;
            }

            INetMessage message = DeserializeObject(payload) as INetMessage;
            if (message == null)
            {
                return;
            }

            if (!_handlers.TryGetValue(messageType, out List<Delegate> handlers))
            {
                return;
            }

            foreach (Delegate handler in handlers.ToArray())
            {
                handler.DynamicInvoke(message, senderId);
            }
        }

        public void RegisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage
        {
            Type messageType = typeof(T);
            if (!_handlers.TryGetValue(messageType, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                _handlers[messageType] = handlers;
            }

            if (!handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        public void UnregisterMessageHandler<T>(MessageHandlerDelegate<T> handler) where T : INetMessage
        {
            Type messageType = typeof(T);
            if (_handlers.TryGetValue(messageType, out List<Delegate> handlers))
            {
                handlers.Remove(handler);
            }
        }

        private string SerializeObject(object value)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, value);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        private object DeserializeObject(string value)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] bytes = Convert.FromBase64String(value);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return formatter.Deserialize(stream);
            }
        }
    }
}