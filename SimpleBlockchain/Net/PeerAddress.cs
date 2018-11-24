using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Net
{
    public class PeerAddress : IEquatable<PeerAddress>
    {
        public byte[] PublicKey { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is PeerAddress other))
                return false;

            return Equals(other);
        }

        public bool Equals(PeerAddress other) => PublicKey.SequenceEqual(other.PublicKey);

        public override int GetHashCode() => PublicKey.Aggregate((first, second) => (byte)(first.GetHashCode() ^ second.GetHashCode()));
    }
}
