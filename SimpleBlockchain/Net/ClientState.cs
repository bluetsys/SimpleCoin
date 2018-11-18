using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Net
{
    enum ClientState
    {
        NotConnected,
        InitializeConnection,
        Connected,
        WaitsConnectionResponse
    }
}
