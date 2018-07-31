using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using IRClib.Definitions;
using IRClib.util;

namespace IRClib {
    public class Client {
        private readonly string _nick;
        // TODO

        public Client(string hostname, int port, string nick, string username, string password, string realname) {
            _nick = nick;
            
            var ip = Dns.GetHostEntry(hostname).AddressList[0];
            //var ip = IPAddress.Parse("85.214.251.202");
            var connection = new Connection(port, ip);
            
            if (String.IsNullOrEmpty(password.Trim()))
                connection.Send("PASS " + password);
            connection.Send("NICK " + _nick);
            connection.Send("USER " + username + " 0 * :" + realname);
            
            connection.Send("JOIN #testchannel");

            Events.RawMessage += ParseRawMessage;
        }

        public static void ParseRawMessage(object o, Events.RawMessageEventArgs args) {
            var message = args.Message;
            if (message[0] != ':') {
                var splitMessage = message.Split(new []{':'}, 2);
                if (splitMessage[0].Contains("PING")) {
                    args.Connection.Send("PONG :" + splitMessage[1]);
                    args.Connection.pinged = true;
                }                
            }
            else {
                message = message.Remove(0, 1);
                //Console.WriteLine("Parsing message: " + message);
                var meta = message.Split(new []{':'}, 2)[0];

                var splitMeta = meta.Split(' ');
                var actor = splitMeta[0];
                var action = splitMeta[1];
                var target = splitMeta[2];

                if (Regex.IsMatch(action, @"^\d+$")) {
                    // is a numeric
                    
                }
                else {
                    // Not a numeric
                    var data = message.Split(new []{':'}, 2)[1];
                    if (action == "PRIVMSG") {
                        new Events().OnMessage(new Events.MessageEventArgs(new Message(new Hostmask(actor), target, data)));
                    }
                }
            }
            
        }
    }
}