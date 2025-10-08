using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace HNTE_Editor
{
    public static class HnteCodecV1
    {
        private static readonly byte[] MAGIC = Encoding.ASCII.GetBytes("HNTE"); // 4 bytes
        private const byte VERSION = 1;
        private const byte ENC_LATIN5 = 1;   // ISO-8859-9
        private const byte ENC_UTF8   = 2;   // fallback
        private const byte XOR_KEY    = 0xA7;

        private static readonly Encoding Latin5 = Encoding.GetEncoding(28599);
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public static void SaveHnte(string path, string text)
        {
            if (text == null) text = string.Empty;

            byte encId;
            byte[] raw;
            try
            {
                raw = Latin5.GetBytes(text);
                encId = ENC_LATIN5;
            }
            catch
            {
                raw = Utf8NoBom.GetBytes(text);
                encId = ENC_UTF8;
            }

            byte[] compressed = Deflate(raw);
            XorInPlace(compressed, XOR_KEY);
            uint crc = Crc32(raw);

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(MAGIC, 0, MAGIC.Length);
            fs.WriteByte(VERSION);
            fs.WriteByte(encId);

            Span<byte> buf = stackalloc byte[4];
            BitConverter.TryWriteBytes(buf, raw.Length);
            fs.Write(buf); // LEN

            BitConverter.TryWriteBytes(buf, crc);
            fs.Write(buf); // CRC

            fs.Write(compressed, 0, compressed.Length);
        }

        public static string LoadHnte(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);

            Span<byte> head = stackalloc byte[4];
            if (fs.Read(head) != 4 || !head.SequenceEqual(MAGIC))
                throw new InvalidDataException("Geçersiz HNTE dosyası (magic).");

            int ver = fs.ReadByte();
            if (ver != VERSION) throw new NotSupportedException($"HNTE sürümü desteklenmiyor: {ver}");

            int encId = fs.ReadByte();
            if (encId != ENC_LATIN5 && encId != ENC_UTF8)
                throw new NotSupportedException($"Bilinmeyen encoding id: {encId}");

            Span<byte> buf = stackalloc byte[4];
            if (fs.Read(buf) != 4) throw new InvalidDataException("Eksik başlık (LEN).");
            int rawLen = BitConverter.ToInt32(buf);

            if (fs.Read(buf) != 4) throw new InvalidDataException("Eksik başlık (CRC).");
            uint crcExpected = BitConverter.ToUInt32(buf);

            byte[] payload = ReadToEnd(fs);
            XorInPlace(payload, XOR_KEY);
            byte[] raw = Inflate(payload, rawLen);

            uint crcActual = Crc32(raw);
            if (crcActual != crcExpected)
                throw new InvalidDataException("CRC doğrulaması başarısız (dosya bozuk olabilir).");

            return encId == ENC_LATIN5 ? Latin5.GetString(raw) : Utf8NoBom.GetString(raw);
        }

        private static void XorInPlace(byte[] data, byte key)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] ^= key;
        }

        private static byte[] Deflate(byte[] input)
        {
            using var ms = new MemoryStream();
            using (var ds = new DeflateStream(ms, CompressionLevel.SmallestSize, leaveOpen: true))
                ds.Write(input, 0, input.Length);
            return ms.ToArray();
        }

        private static byte[] Inflate(byte[] input, int expectedLen)
        {
            using var inMs = new MemoryStream(input);
            using var ds = new DeflateStream(inMs, CompressionMode.Decompress);
            using var outMs = new MemoryStream(expectedLen > 0 ? expectedLen : 0);
            ds.CopyTo(outMs);
            return outMs.ToArray();
        }

        private static uint Crc32(byte[] data)
        {
            unchecked
            {
                uint crc = 0xFFFFFFFFu;
                foreach (byte b in data)
                {
                    crc ^= b;
                    for (int i = 0; i < 8; i++)
                        crc = (crc >> 1) ^ (0xEDB88320u & (uint)((crc & 1) * -1));
                }
                return ~crc;
            }
        }

        private static byte[] ReadToEnd(Stream s)
        {
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}