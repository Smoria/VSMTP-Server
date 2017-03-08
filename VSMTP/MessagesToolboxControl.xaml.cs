namespace VSMTP
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public partial class MessagesToolboxControl : UserControl
    {
        public List<SessionParameters> ServerSessionParameters = new List<SessionParameters>();
        
        private const string IconRunning = "Status_Pause_16xLG.png";
        private const string IconStopped = "Status_Play_16xLG.png";
        private const string IconClear = "Action_Clear_16xLG.png";

        private SMTPServer server;

        public MessagesToolboxControl()
        {
            this.InitializeComponent();

            ServerParameters parameters = new ServerParameters();
            this.DataContext = parameters;

            server = new SMTPServer(parameters);
            parameters.ParametersChanged += Parameters_ParametersChanged;
            server.MessageRecieved += Server_MessageRecieved;
            server.Restart();
            
            UpdateIndicators();
            ClearAll.Source = new BitmapImage(new Uri(GetImageFullPath(IconClear)));
            this.Focus();
        }

        private void Parameters_ParametersChanged(object sender, ParametersEnventArgs e)
        {
            Dispatcher.Invoke(() => UpdateIndicators());
        }

        private void Server_MessageRecieved(object sender, MessageEventArgs e)
        {
            ServerSessionParameters.Add(e.Parameters);
            Dispatcher.Invoke(() =>
            {
                messagesListBox.ItemsSource = null;
                messagesListBox.ItemsSource = ServerSessionParameters;
            });
        }
        public string GetImageFullPath(string filename)
        {
            string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(assemblyLocation, @"Resources", filename);
        }

        private void UpdateIndicators()
        {
            ServerStatusBox.Source = new BitmapImage(new Uri(server.Parameters.Running ? GetImageFullPath(IconRunning) : GetImageFullPath(IconStopped)));
            ServerStatusBox.ToolTip = GetServerStatusString();
        }

        private string GetServerStatusString()
        {
            string message;
            if (server.Parameters.Running)
            {
                message = string.Format(Properties.Resources.MessageBoxServerStatusRunning, (server.Parameters.DisableESMTP ? Properties.Resources.SMTP : Properties.Resources.ESMTP));
            }
            else
            {
                message = string.Format(Properties.Resources.MessageBoxServerStatusStopped);
            }

            return message;
        }

        private void ServerStatusBox_Click(object sender, RoutedEventArgs e)
        {
            string message = GetServerStatusString();

            if (MessageBox.Show(message, Properties.Resources.MessageBoxServerStatusHeader, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (server.Parameters.Running)
                {
                    server.Stop();
                }
                else
                {
                    server.Start();
                }
            }
        }

        private void messagesListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                RemoveSelectedMessage();
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            ServerSessionParameters.Clear();
            messagesListBox.ItemsSource = null;
        }

        private void RemoveSelectedMessage()
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                ServerSessionParameters.RemoveAt(messagesListBox.SelectedIndex);
                messagesListBox.ItemsSource = null;
                messagesListBox.ItemsSource = ServerSessionParameters;
            }
        }

        private void ForceSMTP_Click(object sender, RoutedEventArgs e)
        {
            server.Parameters.DisableESMTP = !server.Parameters.DisableESMTP;
        }

        private void CMenuRemove_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedMessage();
        }

        private void CMenuCopyMessage_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.Message);
                }
            }
        }
        private void CMenuCopyTo_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.MessageTo);
                }
            }
        }

        private void CMenuCopyFrom_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.MessageFrom);
                }
            }
        }

        private void CMenuCopyCC_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.MessageCC);
                }
            }
        }

        private void CMenuCopyReplyTo_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.ReplyTo);
                }
            }
        }

        private void CMenuCopySubject_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.Subject);
                }
            }
        }

        private void CMenuCopyDate_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.Date);
                }
            }
        }

        private void CMenuCopyRAW_Click(object sender, RoutedEventArgs e)
        {
            if (messagesListBox.SelectedIndex >= 0)
            {
                var item = messagesListBox.SelectedItem as SessionParameters;
                if (item != null)
                {
                    Clipboard.SetText(item.Data);
                }
            }
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    int val;
                    if (int.TryParse(textBox.Text, out val))
                    {
                        server.Parameters.Port = val;
                        this.Focus();
                    }
                }
            }
        }
    }
}