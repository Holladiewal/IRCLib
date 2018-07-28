using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using IRClib.util;

namespace IRClib.util {
    using System.Net.Sockets;

    public class MessageEventArgs : EventArgs {
        private readonly string message;

        public MessageEventArgs(string message) {
            this.message = message;
        }

        public string GetMessage() {
            return message;
        }


    }

    public delegate void MessageEventHandler(object o, MessageEventArgs eventArgs);

    public class Connection {

        Socket socket;
        private NetworkStream NetworkStream;
        private string oldRawResponse = "";
        // ReSharper disable once InconsistentNaming
        private readonly Events Events = new Events();
        public Connection(int port, string addr, bool ipv6) : this(port, IPAddress.Parse(addr), ipv6) { }

        public Connection(int port, IPAddress addr, bool ipv6) {

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(addr, port);
            while (!socket.Connected) { }
            NetworkStream = new NetworkStream(socket);
            Thread receiverThread = new Thread(o => {
                while (true) {
                    Receive();
                    Thread.Sleep(500);
                }
            });
            receiverThread.Start();
            
        }

        public void Receive() {
            var buffer = new byte[4096];
            var actuallyRead = NetworkStream.Read(buffer, 0, 4096);
            
            var rawResponse = oldRawResponse + Encoding.UTF8.GetString(buffer);
            var splitResponse = rawResponse.Replace("\n", "").Split('\r');
            if (!splitResponse.Last().EndsWith("\r")) {
                oldRawResponse = splitResponse.Last();
                splitResponse[splitResponse.Length - 1] = "";
            }
            else {
                oldRawResponse = "";
            }
            
            foreach (var str in splitResponse.Except(new []{""})) {
                Events.OnRawMessage(new MessageEventArgs(rawResponse));
            }
            
            
        }

        public void Send(string message) {
            message = message.TrimEnd('\r', '\n').TrimStart('\r', '\n');
        }
    }
}