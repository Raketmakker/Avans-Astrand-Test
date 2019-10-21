﻿#region Imports
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
#endregion

namespace Server
{
    class MessageHandler
    {
        #region Variables
        private ClientConnection clientConnection;
        private Dictionary<string, Action<JObject, TcpClient>> callbacks;
        private FileIOClass fileIO;
        #endregion

        public MessageHandler(ClientConnection clientConnection)
        {
            this.clientConnection = clientConnection;
            callbacks = new Dictionary<string, Action<JObject, TcpClient>>();
            callbacks["data"] = OnData;
            callbacks["dataRequest"] = OnDataRequest;
            fileIO = FileIOClass.GetInstance();
        }

        //Save data when it comes in
        private void OnData(JObject obj, TcpClient client)
        {
            fileIO.Write(obj);
            Console.WriteLine("MessageHandler.OnData: logged the data");
        }

        //Send data when requested
        private void OnDataRequest(JObject obj, TcpClient client)
        {
            var data = new
            {
                type = "dataRequest",
                data = fileIO.Data
            };

            clientConnection.Write(client, JObject.FromObject(data));
            Console.WriteLine("MessageHandler.OnDataRequest: sent data to the client");
        }
        
        //Execute the given function from the callbacks
        private void Invoke(string function, JObject data, TcpClient client)
        {
            Console.WriteLine("MessageHandler.Invoke: " + function.Trim());
            callbacks[function.Trim()].Invoke(data, client);
        }

        //Parse and handle a given message
        public void HandleMessage(string message, TcpClient client)
        {
            try
            {
                JObject data = JObject.Parse(message);
                Invoke(data["type"].ToObject<string>(), data, client);
            }
            catch (JsonReaderException)
            {
                Console.WriteLine("MessageHandler.HandleMessage: JsonReaderException");
            }
        }
    }
}