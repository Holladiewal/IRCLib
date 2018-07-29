using System;
using System.Linq;
using System.Net;
using IRClib.util;

namespace IRClib {
    public class Client {
        private readonly string _nick;
        // TODO

        public Client(string nick, string username, string password) {
            _nick = nick;
            
            const string hostname = "whydoyouhate.me";
            var ip = Dns.GetHostEntry(hostname).AddressList[0];
            //var ip = IPAddress.Parse("85.214.251.202");
            var connection = new Connection(6667, ip);
            
            if (String.IsNullOrEmpty(password.Trim()))
                connection.Send("PASS none");
            connection.Send("NICK IRCLibTestClient");
            connection.Send("USER IRCLib 0 * :I am an IRC Lib");

            Events.RawMessage += ParseRawMessage;
        }

        public static void ParseRawMessage(object o, Events.RawMessageEventArgs args) {
            var message = args.Message;
            if (message[0] == ':') message = message.Remove(0, 1);
            var splitMessage = message.Split(new []{':'}, 2);
            // PING PONG 
            if (splitMessage[0].Contains("PING")) {
                args.Connection.Send("PONG :" + splitMessage[1]);
            }
        }
    }
}