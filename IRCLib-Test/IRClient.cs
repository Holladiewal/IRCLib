using System;
using IRClib;
using IRClib.util;

namespace IRCLib_Test
{
    // ReSharper disable once InconsistentNaming
    
    public class IRClient {
        public static void Main(string[] args) {
            var client = new Client("whydoyouhate.me", 6667, "IRCLibTest", "IRCLib", "", "I am an IRCLib");
            
            Events.RawMessage += OnRawMessage;
            Events.Message += OnMessage;

        }

        private static void OnRawMessage(object o, Events.RawMessageEventArgs args) {
            var message = args.Message;

            if (message.Replace("\0", "").Replace("\n", "").Replace("\r", "").Trim().Length <= 0) return;
            
            Console.WriteLine(message.Trim());
        }

        private static void OnMessage(object o, Events.MessageEventArgs args) {
            var message = args.GetMessage();
            Console.WriteLine("Received parsed Message from " + message.hostmask.ToString() + " in " + message.target + ": " + message.message);
        }
    }
    
}