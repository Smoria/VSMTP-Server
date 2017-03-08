using System.Collections.Generic;
using System.Net.Mime;

namespace VSMTP
{
    public class SessionParameters
    {
        public string ClientName { get; set; } = "Unknown";
        public string ClientAddress { get; set; } = "localhost";
        public string ServerName { get; set; } = "Unknown";
        public int MaxMessageSize { get; set; } = 256;

        public bool Connected { get; set; } = false;
        public bool UsingESMTP { get; set; } = false;

        public List<string> MailFrom { get; set; } = new List<string>();
        public List<string> MailTo { get; set; } = new List<string>();


        public string MIMEVersion { get; set; } = string.Empty;
        public string MessageFrom { get; set; } = string.Empty;
        public string MessageTo { get; set; } = string.Empty;
        public string MessageCC { get; set; } = string.Empty;
        public string ReplyTo { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public ContentType ContentType { get; set; }
        public string ContentTransferEncoding { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;


        public string Data = string.Empty;
        public bool DataReading = false;
        public int UnknownCommandCount = 0;

        public SessionParameters()
        {

        }
    }
}
