using System;

namespace VSMTP
{
    public class ParametersEnventArgs : EventArgs
    {
        public readonly ServerParameters.Parameter Parameter;
        public ParametersEnventArgs(ServerParameters.Parameter p)
        {
            Parameter = p;
        }
    }
}
