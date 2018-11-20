using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Configs
{
    public interface INetworkConfig
    {
        string AddressBookPath { get; }

        int HashLength { get; }
        int RandomNumberLength { get; }

        string PeerHostName { get; }
        int PeerPort { get; }
    }
}
