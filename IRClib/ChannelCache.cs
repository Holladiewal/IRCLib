using System.Collections.Generic;
using IRClib.Definitions;

namespace IRClib {
    public struct ChannelCache {
        private static readonly List<Channel> _channels = new List<Channel>();

        public static Channel ByName(string name) {
            return _channels.Find(channel => channel.Name == name);
        }

        public static void PutChannel(Channel channel) {
            if (!_channels.Exists(chan => chan.Name == channel.Name)) _channels.Add(channel);
        }
    }
}