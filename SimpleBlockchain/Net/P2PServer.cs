using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Net;
using System.Security.Cryptography;

namespace SimpleBlockchain.Net
{
    class P2PServer : WebSocketBehavior
    {
        private WebSocketServer server;

        private void requireAuth()
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] randomNumber = new byte[32];
            byte[] randomNumberHash;

            rng.GetBytes(randomNumber);
        }

        public void Start()
        {
            string hostName = Dns.GetHostName();
            server = new WebSocketServer("ws://" + hostName);

            server.AddWebSocketService<P2PServer>("/simplecoin");
            server.Start();
        }

        public void Stop() => server.Stop();

        protected override void OnMessage(MessageEventArgs e)
        {
            string message = e.Data;
            string[] words = message.Split(new char[] { ' ' });
            /*
            switch (words[0])
            {
                case Commands.ClientHello:
            }*/
        }
    }
}
