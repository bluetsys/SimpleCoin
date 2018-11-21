using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBlockchain.Crypto.Hash
{
    public class ByteConverter : IByteConverter
    {
        public string ConvertToString(byte[] sequence) => String.Join(String.Empty, sequence.Select(number =>
        {
            string hex = number.ToString("x");

            hex = hex.Length == 1 ? "0" + hex : hex;

            return hex;
        }));

        public byte[] ConvertToByteArray(string hex) => Enumerable.Range(0, hex.Length)
                                                        .Where(x => x % 2 == 0)
                                                        .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                                        .ToArray();
    }
}
