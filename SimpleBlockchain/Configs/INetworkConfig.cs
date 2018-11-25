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

        string PeerHostName { get; }
        int PeerPort { get; }

        TimeSpan ClientTimeout { get; }
        TimeSpan ServerTimeout { get; }
    }
}
