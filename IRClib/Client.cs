using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using IRClib.Definitions;
using IRClib.util;

namespace IRClib {
    public class Client {
        private readonly string _nick;
        private static readonly Events Events = new Events();
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

        public void ParseRawMessage(object o, Events.RawMessageEventArgs args) {
            var message = args.Message;
            if (message[0] != ':') {
                var splitMessage = message.Split(new []{':'}, 2);
                // ReSharper disable once InvertIf
                if (splitMessage[0].Contains("PING")) {
                    args.Connection.Send("PONG :" + splitMessage[1]);
                    args.Connection.pinged = true;
                }                
            }
            else {
                message = message.Remove(0, 1);
                //Console.WriteLine("Parsing message: " + message);
                var meta = message.Split(new []{':'}, 2)[0];

                var splitMeta = meta.Split(new []{' '}, 4);
                var actor = splitMeta[0];
                var action = splitMeta[1];
                var target = splitMeta[2];
                var remainder = splitMeta.Length > 3 ? splitMeta[3] : ""; 

                if (Regex.IsMatch(action, @"^\d+$")) {
                    // is a numeric
                    
                }
                else {
                    // Not a numeric

                    var data = message.Contains(":") ? message.Split(new []{':'}, 2)[1] : remainder;

                    switch (action) {
                        case "PRIVMSG": {
                            Events.OnMessage(new Events.MessageEventArgs(new Message(new Hostmask(actor), target, data)));
                            break;
                        }

                        case "MODE": {
                            var senderHostmask =
                                actor.Contains("!") ? new Hostmask(actor) : new Hostmask("", "", actor);
                            IRCObject targetObject = target.StartsWith("#") ? (IRCObject) ChannelCache.ByName(target.Remove(0,1)) : UserCache.ByNick(target);
                            Events.OnModeChangeEvent(new Events.ModeChangeEventArgs(senderHostmask, targetObject, data));
                            break;
                        }

                        case "JOIN": {
                            if (actor.Remove(actor.IndexOf("!", StringComparison.Ordinal)) == _nick) {
                                // WE joined a channel
                                ChannelCache.PutChannel(new Channel(target.Remove(0,1), ""));
                            } else {
                                // somebody else joined a channel we are in
                                var usr = UserCache.ByHostmask(new Hostmask(actor));
                                usr = usr ?? new User(new Hostmask(actor)); 
                                ChannelCache.ByName(target.Remove(0, 1)).AddUser(usr);
                            }

                            break;
                        }

                        case "PART": {
                            if (actor.Remove(actor.IndexOf("!", StringComparison.Ordinal)) == _nick) {
                                // WE left a channel
                                ChannelCache.RemoveChannelByName(target.Remove(0, 1));
                            } else {
                                // somebody else left a channel we are in
                                var usr = UserCache.ByHostmask(new Hostmask(actor));
                                usr = usr ?? new User(new Hostmask(actor)); 
                                ChannelCache.ByName(target.Remove(0, 1)).RemoveUser(usr);
                            }

                            break; 
                        }

                        case "NOTICE": {
                            Events.OnNotice(new Events.MessageEventArgs(new Message(new Hostmask(actor), target, data)));
                            break;
                        }
                    }
                }
            }
        }
    }
}