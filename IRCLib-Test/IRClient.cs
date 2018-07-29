using System;
using System.IO;
using System.Net;
using System.Threading;
using IRClib;
using IRClib.util;

namespace IRCLib_Test
{
    // ReSharper disable once InconsistentNaming
    
    public class IRClient {
        public static void Main(string[] args) {
            var client = new Client("IRCLibTest", "IRCLib", "");
            
            Events.RawMessage += OnRawMessage;

            Thread.Sleep(5000);
            //connection.Send("JOIN #testchannel");
            //connection.Send("PRIVMSG #testchannel :I'm alive!");
        }

        private static void OnRawMessage(object o, Events.RawMessageEventArgs args) {
            var message = args.Message;

            if (message.Replace("\0", "").Replace("\n", "").Replace("\r", "").Trim().Length <= 0) return;
            
            Console.WriteLine(message.Trim().Replace("\0", ""));
        }
    }
    
}