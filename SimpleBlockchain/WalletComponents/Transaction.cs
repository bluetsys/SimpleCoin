using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleBlockchain.Crypto.Hash;
using SimpleBlockchain.Crypto.Signatures;

namespace SimpleBlockchain.WalletComponents
{
    public class Transaction
    {
        public byte[] Nonce { get; set; }
        public byte[] Hash { get; set; }
        public byte[] Signature { get; set; }

        public byte[] Sender { get; set; }
        public byte[] Recipient { get; set; }

        // Change to BigInteger.
        public int Amount { get; set; }

        public Transaction()
        { }

        public Transaction(byte[] sender, byte[] recipient, int amount, IHashFactory hashFactory)
        {
            Sender = sender;
            Recipient = recipient;

            Amount = amount;

            Nonce = hashFactory.GetNonceGenerator().GetNextNonce();
            Hash = ComputeHash(hashFactory);
        }
        
        public byte[] ComputeHash(IHashFactory hashFactory)
        {
            IByteConverter converter = hashFactory.GetByteConverter();
            string rawData = converter.ConvertToString(Nonce) + converter.ConvertToString(Sender) + converter.ConvertToString(Recipient) + Amount;
            byte[] data = Encoding.ASCII.GetBytes(rawData);

            return hashFactory.GetDigest().GetHash(data);
        }

        public void SignTransaction(ISignatureProvider signer) => Signature = signer.SignHash(Hash);

        public bool VerifyTransactionSignature(ISignatureVerifier verifier) => verifier.VerifyHash(Sender, Hash, Signature);
    }
}
