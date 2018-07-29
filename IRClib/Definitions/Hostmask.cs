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
    }
}