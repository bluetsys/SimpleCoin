using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Signatures
{
    public interface ISignatureFactory
    {
        ISignatureProvider GetSignatureProvider();
        ISignatureVerifier GetSignatureVerifier();

        ISignatureProvider GetSignatureProvider(byte[] keyPairBlob);
    }
}
