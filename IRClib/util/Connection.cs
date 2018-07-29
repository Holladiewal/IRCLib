using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IRClib.util {
    public class Connection {

        Socket socket;
        private NetworkStream NetworkStream;
        private string oldRawResponse = "";
        // ReSharper disable once InconsistentNaming
        private readonly Events Events = new Events();
        private readonly int port;
        private readonly IPAddress addr;
        
        public Connection(int port, string addr) : this(port, IPAddress.Parse(addr)) { }

        public Connection(int port, IPAddress addr) {
            Console.WriteLine(addr.AddressFamily.ToString());
            socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.port = port;
            this.addr = addr;
            Connect();
        }

        private void Connect() {
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
                Events.OnRawMessage(new Events.RawMessageEventArgs(rawResponse, this));
            }
            
            //TODO ParseMessage
        }

        public void Send(string message) {
            message = message.Trim('\r', '\n') + "\r\n";
            var sendBuffer = Encoding.UTF8.GetBytes(message);
            NetworkStream.Write(sendBuffer, 0, sendBuffer.Length);
        }
    }
}