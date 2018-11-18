using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleBlockchain.Crypto.Hash;
using System.Numerics;
using System.Collections;

namespace SimpleBlockchain.Crypto.Hash
{
    public class BigIntegerNonceGenerator : INonceGenerator
    {
        private int nonceLength;
        private BigInteger innerNumber;

        public int NonceLength
        {
            get => nonceLength;

            set
            {
                if (value <= 0)
                    throw new ArgumentException("Value must be positive");

                nonceLength = value;
            }
        }

        public BigIntegerNonceGenerator(int nonceLength)
        {
            NonceLength = nonceLength;
            innerNumber = 0;
        }

        public byte[] GetNextNonce()
        {
            byte[] nonce = new byte[NonceLength];
            byte[] nonceValue = innerNumber++.ToByteArray();

            if (nonceValue.Length > NonceLength)
                throw new InvalidOperationException("There is no more appropriate nonce");

            Array.Copy(nonceValue, 0, nonce, NonceLength - nonceValue.Length, nonceValue.Length);

            return nonce;
        }

        public void Reset() => innerNumber = 0;

        public IEnumerator<byte[]> GetEnumerator()
        {
            for (BigInteger current = 0; NonceLength >= current.ToByteArray().Length; current++)
            {
                byte[] nonce = new byte[NonceLength];
                byte[] currentValue = current.ToByteArray();

                Array.Copy(currentValue, 0, nonce, NonceLength - currentValue.Length, currentValue.Length);

                yield return nonce;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
