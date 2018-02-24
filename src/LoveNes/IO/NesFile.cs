using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LoveNes.IO
{
    public class NesFile
    {
        private readonly byte[] _content;

        public uint PrgRomSize { get; private set; }

        public uint ChrRomSize { get; private set; }

        public NesFile(byte[] content)
        {
            _content = content;

            Load();
        }

        private static readonly uint _magicNumber = 0x4E45531A;

        private void Load()
        {
            var span = new SpanReader(_content);
            if (span.ReadAsUnsignedInt() != _magicNumber)
                throw new InvalidDataException("Invalid nes file.");
            PrgRomSize = span.ReadAsByte() * 16u * 1024;
            ChrRomSize = span.ReadAsByte() * 8u * 1024;
        }

        public static async Task<NesFile> FromStream(Stream stream)
        {
            var content = new byte[stream.Length];
            await stream.ReadAsync(content, 0, content.Length);
            return new NesFile(content);
        }
    }
}
