using System;
using System.IO;
using System.Net;
using IRClib.util;

namespace IRCLib_Test
{
    // ReSharper disable once InconsistentNaming
    
    public class IRClient {
        static StreamWriter Streamwriter = new StreamWriter(File.OpenWrite("debug.log"));
        public static void Main(string[] args) {
            const string hostname = "whydoyouhate.me";
            //var ip = Dns.GetHostEntry(hostname).AddressList[0];
            var ip = IPAddress.Parse("85.214.251.202");
            var connection = new Connection(6667, ip, false);
            Events.RawMessage += OnRawMessage;
        }

        private static void OnRawMessage(object o, MessageEventArgs args) {
            var message = args.GetMessage();
            if (string.IsNullOrEmpty(message) || message.Trim().Equals("")) {
                return;
            }

            if (message.Replace("\n", "").Replace("\r", "").Trim().Length <= 0) return;
            
            
            //Streamwriter.Write(message.Trim().Replace("\0", ""));
            //Streamwriter.Flush();
            Console.WriteLine(message.Trim().Replace("\0", ""));
        }
    }
    
}