using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace VSMTP
{
    public class SMTPServer
    {
        public event EventHandler<MessageEventArgs> MessageRecieved;        
        public readonly ServerParameters Parameters;

        private TcpListener listener;
        private Thread listenThread;
        private ManualResetEvent resetEvent = new ManualResetEvent(true);

        public SMTPServer(ServerParameters parameters)
        {
            Parameters = parameters;
            Parameters.Running = false;
            Parameters.ParametersChanged += Parameters_ParametersChanged;

            listenThread = new Thread(new ThreadStart(ListenThread));
            listenThread.Start();
        }

        private void Parameters_ParametersChanged(object sender, ParametersEnventArgs e)
        {
            if(e.Parameter == ServerParameters.Parameter.Port)
            {
                Restart();
            }
        }

        public void Stop()
        {
            resetEvent.Reset();
            Parameters.Running = false;

            if (listener != null)
            {
                listener.Stop();
            }
        }

        public void Start()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Parameters.Port);
            try
            {
                listener = new TcpListener(endPoint);
                listener.Start();
                Parameters.Running = true;
                resetEvent.Set();
            }
            catch (Exception)
            {
                Parameters.Running = false;
            }
        }

        public void Restart()
        {
            Stop();
            Start();
        }

        private void ListenThread()
        {
            while (true)
            {
                resetEvent.WaitOne();

                if (listener != null)
                {
                    try
                    {
                        Parameters.Running = true;
                        TcpClient client = listener.AcceptTcpClient();
                        Thread thread = new Thread(new ParameterizedThreadStart(ProcessClient));
                        thread.Start(client);
                    }
                    catch(Exception)
                    {
                        Parameters.Running = false;
                    }
                }

                Thread.Sleep(500);
            }
        }

        private void ProcessClient(object obj)
        {
            TcpClient client = obj as TcpClient;
            if (client == null)
            {
                return;
            }

            SessionParameters parameters = new SessionParameters()
            {
                ServerName = Parameters.ServerName,
                MaxMessageSize = Parameters.MaxMessageSize,
                ClientAddress = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()
            };

            NetworkStream stream = client.GetStream();
            if (stream == null || !stream.CanRead || !stream.CanWrite)
            {
                return;
            }
            stream.ReadTimeout = 500;

            SendMessage(stream, ServerCommands.CommandGreeting, parameters);

            while (ParseMessage(stream, parameters)) ;

            client.Close();

            if (MessageRecieved != null)
            {
                MessageRecieved(this, new MessageEventArgs(parameters));
            }
        }

        private bool ParseMessage(NetworkStream stream, SessionParameters parameters)
        {
            string message = ReadMessage(stream);

            if (message == null)
            {
                return false;
            }

            if (parameters.DataReading)
            {
                if (string.Equals(message, ".\r\n"))
                {
                    SendMessage(stream, ServerCommands.CommandDataAccepted, parameters);
                    parameters.DataReading = false;
                    if (string.Equals(parameters.ContentTransferEncoding, "base64", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Encoding encoding = Encoding.ASCII;
                        if (parameters.ContentType != null)
                        {
                            try
                            {
                                encoding = Encoding.GetEncoding(parameters.ContentType.CharSet);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        try
                        {
                            parameters.Message = encoding.GetString(Convert.FromBase64String(parameters.Message));
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
                else
                {
                    int index = message.IndexOf(":");
                    if (index >= 0)
                    {
                        string data = message.Remove(0, index).Trim(new char[] { ':', ' ', '\r', '\n' });
                        if (message.StartsWith("MIME-Version"))
                        {
                            parameters.MIMEVersion = data;
                        }
                        else if (message.StartsWith("From"))
                        {
                            if (!string.IsNullOrEmpty(parameters.MessageFrom))
                            {
                                parameters.MessageFrom += ", ";
                            }
                            parameters.MessageFrom += data;
                        }
                        else if (message.StartsWith("To"))
                        {
                            if (!string.IsNullOrEmpty(parameters.MessageTo))
                            {
                                parameters.MessageTo += ", ";
                            }
                            parameters.MessageTo += data;
                        }
                        else if (message.StartsWith("Cc"))
                        {
                            if (!string.IsNullOrEmpty(parameters.MessageCC))
                            {
                                parameters.MessageCC += ", ";
                            }
                            parameters.MessageCC += data;
                        }
                        else if (message.StartsWith("Reply-To"))
                        {
                            if(!string.IsNullOrEmpty(parameters.ReplyTo))
                            {
                                parameters.ReplyTo += ", ";
                            }
                            parameters.ReplyTo += data;
                        }
                        else if (message.StartsWith("Date"))
                        {
                            parameters.Date = data;
                        }
                        else if (message.StartsWith("Subject"))
                        {
                            parameters.Subject = data;
                        }
                        else if (message.StartsWith("Content-Type"))
                        {
                            parameters.ContentType = new ContentType(data);
                        }
                        else if (message.StartsWith("Content-Transfer-Encoding"))
                        {
                            parameters.ContentTransferEncoding = data;
                        }
                        else
                        {
                            parameters.Message += message;
                        }
                    }
                    else
                    {
                        parameters.Message += message;
                    }
                    parameters.Data += message;
                }
                return true;
            }

            if (message.StartsWith("EHLO"))
            {
                if (Parameters.DisableESMTP)
                {
                    SendMessage(stream, ServerCommands.CommandUnknown, parameters);
                }
                else
                {
                    parameters.UsingESMTP = true;
                    parameters.Connected = true;

                    try
                    {
                        parameters.ClientName = message.Remove(0, 5).TrimEnd(new char[] { '\r', '\n' });
                    }
                    catch (Exception)
                    {
                        parameters.ClientName = "Unknown";
                    }

                    SendMessage(stream, ServerCommands.CommandESMTPGreeting, parameters);
                    SendMessage(stream, ServerCommands.CommandESMTPSize, parameters);
                    SendMessage(stream, ServerCommands.CommandESMTPEhlo, parameters);
                    SendMessage(stream, ServerCommands.CommandGetDomain, parameters);
                }
            }
            else if (message.StartsWith("HELO"))
            {
                parameters.UsingESMTP = false;
                parameters.Connected = true;

                SendMessage(stream, ServerCommands.CommandGetDomain, parameters);
            }
            else
            {
                if (!parameters.Connected)
                {
                    return false;
                }

                if (message.StartsWith("MAIL"))
                {
                    try
                    {
                        parameters.MailFrom.Add(message.Remove(0, 11).Trim(new char[] { '\r', '\n', '<', '>' }));
                        SendMessage(stream, ServerCommands.CommandFromAccepted, parameters);
                    }
                    catch (Exception)
                    {
                        SendMessage(stream, ServerCommands.CommandFromDeclined, parameters);
                    }
                }
                else if (message.StartsWith("RCPT"))
                {
                    try
                    {
                        parameters.MailTo.Add(message.Remove(0, 9).Trim(new char[] { '\r', '\n', '<', '>' }));

                        if (!parameters.UsingESMTP || !stream.DataAvailable)
                        {
                            SendMessage(stream, ServerCommands.CommandToAccepted, parameters);
                        }
                    }
                    catch (Exception)
                    {
                        SendMessage(stream, ServerCommands.CommandToDeclined, parameters);
                    }
                }
                else if (message.StartsWith("DATA"))
                {
                    parameters.DataReading = true;
                    SendMessage(stream, ServerCommands.CommandDataAcceptStarted, parameters);

                }
                else if (message.StartsWith("QUIT"))
                {
                    SendMessage(stream, ServerCommands.CommandQuit, parameters);
                    return false;
                }
                else
                {
                    if (++parameters.UnknownCommandCount > Parameters.MaxUnknownCommandCount)
                    {
                        return false;
                    }
                    SendMessage(stream, ServerCommands.CommandUnknown, parameters);
                }
            }
            return true;
        }

        private string FormatCommand(string command, SessionParameters parameters)
        {
            var props = parameters.GetType().GetProperties();
            foreach (var prop in props)
            {
                object value = prop.GetValue(parameters);
                if (value != null)
                {
                    if (prop.PropertyType.GetInterfaces().Contains(typeof(IList)))
                    {
                        IList list = value as IList;
                        if (list.Count > 0)
                        {
                            command = command.Replace("{" + prop.Name + "}", list[list.Count - 1].ToString());
                        }
                        else
                        {
                            command = command.Replace("{" + prop.Name + "}", "Unknown");
                        }
                    }
                    else
                    {
                        command = command.Replace("{" + prop.Name + "}", value.ToString());
                    }
                }
            }
            return command;
        }

        private bool SendMessage(NetworkStream stream, string message, SessionParameters parameters)
        {
            message = FormatCommand(message, parameters);
            return SendMessage(stream, message);
        }

        private bool SendMessage(NetworkStream stream, string message)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private string ReadMessage(NetworkStream stream)
        {
            string message = string.Empty;
            try
            {
                int value = -1;
                while (!message.EndsWith("\r\n") && (value = stream.ReadByte()) >= 0)
                {
                    message += Convert.ToChar(value);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return message;
        }

        ~SMTPServer()
        {
            if (listener != null)
            {
                resetEvent.Reset();
                listener.Stop();
            }
        }
    }
}
