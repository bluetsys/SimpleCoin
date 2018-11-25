using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Configs.Parameters
{
    public class NetworkParameters
    {
        public string AddressBookPath { get; set; }

        public string PeerHostName { get; set; }
        public int PeerPort { get; set; }

        public TimeSpan ClientTimeout { get; set; }
        public TimeSpan ServerTimeout { get; set; }
    }
}
