using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SimpleBlockchain.Crypto.Signatures
{
    public class ECDSASignatureVerifier : ISignatureVerifier
    {
        public bool VerifyHash(byte[] publicKey, byte[] hash, byte[] signature)
        {
            using (ECDsaCng verifier = new ECDsaCng(CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob)))
                return verifier.VerifyHash(hash, signature);
        }
    }
}
