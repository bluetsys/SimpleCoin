using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Signatures
{
    public interface ISignatureVerifier
    {
        bool VerifyHash(byte[] publicKey, byte[] hash, byte[] signature);
    }
}
