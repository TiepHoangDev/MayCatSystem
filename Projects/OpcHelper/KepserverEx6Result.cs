using System;
using System.Collections.Generic;
using System.Text;

namespace OpcHelper
{
    public class KepserverEx6Result<T> : BaseResult<T>
    {
        public StatusInfoKepex Status { get; set; }

        public override string ToString()
        {
            return base.ToString() + $"\tStatus={Status}";
        }
    }
}
