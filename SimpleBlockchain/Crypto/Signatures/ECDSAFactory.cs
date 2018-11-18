using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Signatures
{
    public class ECDSAFactory : ISignatureFactory
    {
        private Lazy<ECDSASignatureVerifier> signatureVerifier;

        public ECDSAFactory()
        {
            signatureVerifier = new Lazy<ECDSASignatureVerifier>(() => new ECDSASignatureVerifier());
        }

        public ISignatureProvider GetSignatureProvider() => new ECDSASignatureProvider();

        public ISignatureProvider GetSignatureProvider(byte[] keyPairBlob) => new ECDSASignatureProvider(keyPairBlob);

        public ISignatureVerifier GetSignatureVerifier() => signatureVerifier.Value;
    }
}
