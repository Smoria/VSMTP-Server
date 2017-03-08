namespace VSMTP
{
    public static class ServerCommands
    {
        public const string CommandGreeting = "220 {ServerName} is glad to see you!\r\n";
        public const string CommandESMTPGreeting = "250-{ServerName} Hello {ClientName}[{ClientAddress}]\r\n";
        public const string CommandESMTPSize = "250-SIZE {MaxMessageSize}\r\n";
        public const string CommandESMTPEhlo = "250 EHLO\r\n";
        public const string CommandGetDomain = "250 domain name should be qualified\r\n";

        public const string CommandFromAccepted = "250 Ok: {MailFrom} sender accepted\r\n";
        public const string CommandFromDeclined = "451 sender parsing error declined\r\n";

        public const string CommandToAccepted = "250 Ok: {MailTo} recipient accepted\r\n";
        public const string CommandToDeclined = "451 recipient parsing error declined\r\n";

        public const string CommandDataAcceptStarted = "354 Enter mail, end with \".\" on a line by itself\r\n";
        public const string CommandDataAccepted = "250 Ok: Message accepted for delivery\r\n";

        public const string CommandQuit = "221 {ServerName} closing connection\r\n";
        public const string CommandUnknown = "500 Unknown\r\n";
    }
}
