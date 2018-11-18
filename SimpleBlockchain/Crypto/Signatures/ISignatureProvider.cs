using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Signatures
{
    public interface ISignatureProvider
    {
        byte[] PublicKey { get; }

        byte[] SignHash(byte[] hash);
        byte[] ExportKeyPairBlob();
    }
}
