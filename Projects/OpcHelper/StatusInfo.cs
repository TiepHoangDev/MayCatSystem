using OpcLabs.BaseLib.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OpcHelper
{
    public enum StatusInfoKepex
    {
        [Description("Unknown status.")]
        Unknown,
        [Description("Normal status.")]
        Normal,
        [Description("Warning status.")]
        Warning,
        [Description("Error status.")]
        Error
    }

    public static class EnumHelper
    {
        public static StatusInfoKepex ToStatusInfoKepex(this StatusInfo statusInfo)
        {
            return (StatusInfoKepex)statusInfo;
        }
    }
}
