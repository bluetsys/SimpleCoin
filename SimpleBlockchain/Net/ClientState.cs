using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Net
{
    public enum ClientState
    {
        NotConnected,
        InitializeConnection,
        Connected,
        WaitsConnectionResponse,
        WaitsUnitHash,
        WaitsUnit
    }
}
