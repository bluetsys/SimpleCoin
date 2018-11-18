using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace MerkleTree
{
    public class MerkleTreeBuilder
    {
        public byte[] GetMerkleRoot(IEnumerable<byte[]> hashes, HashAlgorithm hashAlgorithm)
        {
            if (hashes.Count() % 2 == 1)
                throw new ArgumentException("Hash collection must have even number of elements");

            Queue<byte[]> hashTraverseQueue = new Queue<byte[]>(hashes);
            byte[] merkleRoot;

            while (hashTraverseQueue.Count > 1)
            {
                byte[] firstHash = hashTraverseQueue.Dequeue();
                byte[] secondHash = hashTraverseQueue.Dequeue();
                byte[] tempBuffer = new byte[firstHash.Length + secondHash.Length];

                Array.Copy(firstHash, 0, tempBuffer, 0, firstHash.Length);
                Array.Copy(secondHash, 0, tempBuffer, firstHash.Length, secondHash.Length);

                byte[] newHash = hashAlgorithm.ComputeHash(tempBuffer);

                hashTraverseQueue.Enqueue(newHash);
            }

            merkleRoot = hashTraverseQueue.Dequeue();

            return merkleRoot;
        }
    }
}
