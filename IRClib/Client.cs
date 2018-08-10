using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IRClib.Definitions;
using IRClib.util;

namespace IRClib {
    public class Client {
        private readonly string _nick;
        private static readonly Events Events = new Events();
        private List<string> _acknowledgedCapabilities = new List<string>();
        private List<string> _supportedCapabilities = new List<string>();

        public Client(string hostname, int port, string nick, string username, string password, string realname,
                      bool ssl, bool sasl, string saslMechanism = "", string certpath = "") {
            _nick = nick;
            
            var ip = Dns.GetHostEntry(hostname).AddressList[0];
            var connection = new Connection(port, ip, ssl, sasl, certpath);
            
            Events.RawMessage += ParseRawMessage;
            
            if (!String.IsNullOrEmpty(password.Trim()) && !sasl)
                connection.Send("PASS " + password);
            connection.Send("NICK " + _nick);
            connection.Send("USER " + username + " 0 * :" + realname);

            connection.Send("CAP LS 302");
            
            if (sasl) {
                while (_supportedCapabilities.Count < 1) ;
                if (_supportedCapabilities.Contains("sasl"))
                    connection.Send("CAP REQ :sasl");
                else return;
                while (!_acknowledgedCapabilities.Contains("sasl")) {
                    var capstring = "";
                    _acknowledgedCapabilities.ForEach(strin => capstring += $"{strin} ");
                    //Console.WriteLine($"Acknowledged Capabilities while waiting for sasl: {capstring}");
                }
                connection.Send($"AUTHENTICATE {saslMechanism}");
                switch (saslMechanism) {
                    case "EXTERNAL": {
                        break;
                    }
                    
                    case "PLAIN": {
                        connection.Send($"AUTHENTICATE {Convert.ToBase64String(Encoding.UTF8.GetBytes(password))}");
                        break;
                    }
                    
                    default: throw new NotSupportedException($"SASL MECHANISM '{saslMechanism}' is not supported'");
                }
                connection.Send("AUTHENTICATE +");
            }
            connection.Send("CAP END");
            
            // connection.Send("JOIN #testchannel");


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
                                Console.WriteLine($"target string is: {data}");
                                ChannelCache.PutChannel(new Channel(data.Remove(0,1), ""));
                            } else {
                                // somebody else joined a channel we are in
                                var usr = UserCache.ByHostmask(new Hostmask(actor));
                                usr = usr ?? new User(new Hostmask(actor)); 
                                ChannelCache.ByName(data.Remove(0, 1)).AddUser(usr);
                            }

                            break;
                        }

                        case "PART": {
                            if (actor.Remove(actor.IndexOf("!", StringComparison.Ordinal)) == _nick) {
                                // WE left a channel
                                ChannelCache.RemoveChannelByName(data.Remove(0, 1));
                            } else {
                                // somebody else left a channel we are in
                                var usr = UserCache.ByHostmask(new Hostmask(actor));
                                usr = usr ?? new User(new Hostmask(actor)); 
                                ChannelCache.ByName(data.Remove(0, 1)).RemoveUser(usr);
                            }

                            break; 
                        }

                        case "NOTICE": {
                            Events.OnNotice(new Events.MessageEventArgs(new Message(new Hostmask(actor), target, data)));
                            break;
                        }

                        case "CAP": {
                            switch (remainder.Split(' ')[0]) {
                                case "ACK":
                                    // foreach (var str in data.Split(' ')) { _acknowledgedCapabilities.Add(str); }
                                    _acknowledgedCapabilities.AddRange(data.Split(' '));
                                    Events.OnCapAckEvent(new Events.StringEventArgs(data));
                                    
                                    var capstring_ACK = "";
                                    _acknowledgedCapabilities.ForEach(strin => capstring_ACK += $"{strin} ");
                                    Console.WriteLine($"Caps acknowledged: {capstring_ACK}");
                                    
                                    break;
                                case "NAK": 
                                    Events.OnCapNakEvent(new Events.StringEventArgs(data));
                                    break;
                                
                                case "LS":
                                    Console.WriteLine("INCOMING CAP LIST!");
                                    _supportedCapabilities.AddRange(data.Split(' '));
                                    if (!(remainder.Length > 2 && remainder.Split(' ')[1] == "*")) {
                                        var capstring = "";
                                        _supportedCapabilities.ForEach(strin => capstring += $"{strin} ");
                                            
                                        Events.OnCapLsEvent(new Events.StringEventArgs(capstring));
                                        Console.WriteLine($"END OF CAP LIST, SUPPORTED CAPS: {capstring}");
                                    }

                                    break;
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}