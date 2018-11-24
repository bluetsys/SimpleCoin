using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBlockchain.Net
{
    public interface IAddressBook : IEnumerable<KeyValuePair<PeerAddress, string>>
    {
        void Add(PeerAddress address, string url);
        void Remove(PeerAddress address);
    }
}
