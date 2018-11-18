using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Hash
{
    public interface INonceGenerator : IEnumerable<byte[]>
    {
        int NonceLength { get; set; }
        
        byte[] GetNextNonce();
        void Reset();
    }
}
