using System;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;

namespace VSMTP
{
    public class ServerParameters
    {
        public enum Parameter
        {
            Port,
            MaxMessageSize,
            MaxUnknownCommandCount,
            ServerName,
            DisableESMTP,
            Running
        }

        public event EventHandler<ParametersEnventArgs> ParametersChanged;

        private int port = 25;
        private int maxMessageSize = 14680064;
        private int maxUnknownCommandCount = 10;
        private string serverName = "VSMTP";
        private bool disableESMTP = false;
        private bool running = false;

        public int Port
        {
            get { return port; }
            set { port = value; RaiseParametersChanged(Parameter.Port); }
        }
        public int MaxMessageSize
        {
            get { return maxMessageSize; }
            set { maxMessageSize = value; RaiseParametersChanged(Parameter.MaxMessageSize); }
        }
        public int MaxUnknownCommandCount
        {
            get { return maxUnknownCommandCount; }
            set { maxUnknownCommandCount = value; RaiseParametersChanged(Parameter.MaxUnknownCommandCount); }
        }
        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; RaiseParametersChanged(Parameter.ServerName); }
        }
        public bool DisableESMTP
        {
            get { return disableESMTP; }
            set { disableESMTP = value; RaiseParametersChanged(Parameter.DisableESMTP); }
        }

        public bool Running
        {
            get { return running; }
            set { running = value; RaiseParametersChanged(Parameter.Running); }
        }
        private void RaiseParametersChanged(Parameter param)
        {
            if (ParametersChanged != null)
            {
                ParametersChanged(this, new ParametersEnventArgs(param));
            }

            if (param == Parameter.Port || param == Parameter.DisableESMTP)
            {
                SettingsManager settingsManager = new ShellSettingsManager(VMSTPSettingsStore.ServiceProvider);
                WritableSettingsStore configurationSettingsStore = settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

                bool collectionExists = configurationSettingsStore.CollectionExists(VMSTPSettingsStore.Collection);
                if (!collectionExists)
                {
                    configurationSettingsStore.CreateCollection(VMSTPSettingsStore.Collection);
                }

                if (collectionExists || configurationSettingsStore.CollectionExists(VMSTPSettingsStore.Collection))
                {
                    configurationSettingsStore.SetBoolean(VMSTPSettingsStore.Collection, "DisableESMTP", DisableESMTP);
                    configurationSettingsStore.SetInt32(VMSTPSettingsStore.Collection, "Port", Port);
                }
            }
        }

        public ServerParameters()
        {
            SettingsManager settingsManager = new ShellSettingsManager(VMSTPSettingsStore.ServiceProvider);
            SettingsStore configurationSettingsStore = settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            if (configurationSettingsStore.CollectionExists(VMSTPSettingsStore.Collection))
            {
                disableESMTP = configurationSettingsStore.GetBoolean(VMSTPSettingsStore.Collection, "DisableESMTP", DisableESMTP);
                port = configurationSettingsStore.GetInt32(VMSTPSettingsStore.Collection, "Port", Port);
            }
        }
    }
}
