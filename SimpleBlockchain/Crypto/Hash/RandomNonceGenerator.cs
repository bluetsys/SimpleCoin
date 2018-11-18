using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Security.Cryptography;
using System.Collections;

namespace SimpleBlockchain.Crypto.Hash
{
    public class RandomNonceGenerator : INonceGenerator
    {
        private RNGCryptoServiceProvider random;

        public int NonceLength { get; set; }

        public RandomNonceGenerator(int nonceLength)
        {
            NonceLength = nonceLength;
            random = new RNGCryptoServiceProvider();
        }
        
        public byte[] GetNextNonce()
        {
            byte[] nonce = new byte[NonceLength];

            random.GetBytes(nonce);

            return nonce;
        }

        public void Reset() => random = new RNGCryptoServiceProvider();

        public IEnumerator<byte[]> GetEnumerator()
        {
            for (BigInteger i = 0; i < BigInteger.Pow(2, NonceLength * 8); i++)
            {
                byte[] nonce = new byte[NonceLength];

                random.GetBytes(nonce);

                yield return nonce;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
