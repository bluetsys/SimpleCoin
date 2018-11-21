using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Hash
{
    public interface IDigest
    {
        int HashLength { get; }

        byte[] GetHash(byte[] data);
    }
}
