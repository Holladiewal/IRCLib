using System;
using System.Text.RegularExpressions;

namespace IRClib.Definitions {
    public class Hostmask {
        private readonly string nickname;
        private readonly string ident;
        private readonly string hostname;

        public Hostmask(string nickname, string ident, string hostname) {
            this.nickname = nickname;
            this.ident = ident;
            this.hostname = hostname;
        }

        public Hostmask(string hostmask) {
            var result = Regex.Match(hostmask, @"(?<nickname>\w+?)!(?<ident>[\d\w]+?)@(?<hostname>[\w\d-\.]+)");
            
            nickname = result.Groups["nickname"].Value;
            ident = result.Groups["ident"].Value;
            hostname = result.Groups["hostname"].Value;
            
        }

        public override string ToString() {
            return String.Format("{0}!{1}@{2}", nickname, ident, hostname);
        }
    }
}