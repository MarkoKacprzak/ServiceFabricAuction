using System;

namespace Richter.Utilities {
   public sealed class Crc32 {
      // http://www.w3.org/TR/PNG-CRCAppendix.html
      private const uint c_allOnes = 0xffffffff; // CRC is initialized to all 1s
      private static readonly uint[] s_CrcTable = new uint[256];

      static Crc32() {
         for (uint n = 0; n < 256; n++) {
            var remainder = n;   // Remainder from polynomial division
            for (uint k = 0; k < 8; k++) {
               if ((remainder & 1) != 0) remainder = 0xedb88320 ^ (remainder >> 1);
               else remainder >>= 1;
            }
            s_CrcTable[n] = remainder;
         }
      }

      public uint Start(byte[] bytes, int start = 0, int length = -1)
         => Update(c_allOnes, bytes, start, length);

      public uint Update(uint crc, byte[] bytes, int start = 0, int length = -1) {
         if (length == -1) length = bytes.Length - start;
         for (uint n = 0; n < length; n++)
            crc = s_CrcTable[(crc ^ bytes[start + n]) & 0xff] ^ (crc >> 8);
         return crc;
      }
      public uint Finish(byte[] bytes, int start = 0, int length = -1)
         => Finish(Start(bytes, start, length));

      public uint Finish(uint crc, byte[] bytes, int start = 0, int length = -1)
         => Finish(Update(crc, bytes, start, length));
      public uint Finish(uint crc) => crc ^ c_allOnes;  // Final is the 1's compliment
   }
}
