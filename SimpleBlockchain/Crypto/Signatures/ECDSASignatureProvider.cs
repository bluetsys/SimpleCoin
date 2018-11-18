using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SimpleBlockchain.Crypto.Signatures
{
    class ECDSASignatureProvider : ISignatureProvider
    {
        private ECDsaCng innerSigner;
        
        public byte[] PublicKey { get; }

        public ECDSASignatureProvider()
        {
            innerSigner = new ECDsaCng();
            PublicKey = innerSigner.Key.Export(CngKeyBlobFormat.EccPublicBlob);
        }

        public ECDSASignatureProvider(byte[] keyPairBlob)
        {
            innerSigner = new ECDsaCng(CngKey.Import(keyPairBlob, CngKeyBlobFormat.EccPrivateBlob));
            PublicKey = innerSigner.Key.Export(CngKeyBlobFormat.EccPublicBlob);
        }

        public byte[] SignHash(byte[] hash) => innerSigner.SignHash(hash);

        public byte[] ExportKeyPairBlob() => innerSigner.Key.Export(CngKeyBlobFormat.EccPrivateBlob);
    }
}
