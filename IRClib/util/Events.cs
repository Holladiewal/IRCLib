namespace IRClib.util {
    public class Events {
        public static event MessageEventHandler Message;
        public static event MessageEventHandler RawMessage;

        public void OnMessage(MessageEventArgs args) {
            if (Message != null) Message(this, args);
        }

        public void OnRawMessage(MessageEventArgs args) {
            if (RawMessage != null) RawMessage(this, args);
        }
    }
}