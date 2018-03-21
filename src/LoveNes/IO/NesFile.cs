using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LoveNes.IO
{
    public class NesFile
    {
        public uint PrgRomSize { get; private set; }

        public uint ChrRomSize { get; private set; }

        public uint PrgRamSize { get; private set; }

        public bool HasTrainer { get; private set; }

        public byte[] Trainer { get; private set; }

        public byte[] PrgRom { get; private set; }

        public byte[] ChrRom { get; private set; }

        public MirroringMode MirroringMode { get; private set; }

        public NesFile(byte[] content)
        {
            Load(content);
        }

        private static readonly uint _magicNumber = 0x4E45531A;

        private void Load(byte[] content)
        {
            var span = new SpanReader(content);
            ReadHeader(ref span);
            ReadTrainer(ref span);
            ReadPrgRom(ref span);
            ReadChrRom(ref span);

            Debug.Assert(span.IsCosumed, "nes not consumed.");
        }

        private void ReadChrRom(ref SpanReader span)
        {
            ChrRom = span.ReadAsByteArray((int)ChrRomSize);
        }

        private void ReadPrgRom(ref SpanReader span)
        {
            PrgRom = span.ReadAsByteArray((int)PrgRomSize);
        }

        private void ReadTrainer(ref SpanReader span)
        {
            if (HasTrainer)
                Trainer = span.ReadAsByteArray(512);
        }

        private void ReadHeader(ref SpanReader span)
        {
            if (span.ReadAsUnsignedInt() != _magicNumber)
                throw new InvalidDataException("Invalid nes file.");
            PrgRomSize = span.ReadAsByte() * 16u * 1024;
            ChrRomSize = span.ReadAsByte() * 8u * 1024;

            var flag6 = span.ReadAsByte();
            MirroringMode = (MirroringMode)(flag6 & 0b1);
            HasTrainer = (flag6 & 0b001) != 0;

            var flag7 = span.ReadAsByte();

            PrgRamSize = Math.Max((byte)1, span.ReadAsByte()) * 8u * 1024;
            var flag9 = span.ReadAsByte();
            var flag10 = span.ReadAsByte();

            span.Advance(5);
        }

        public static async Task<NesFile> FromStream(Stream stream)
        {
            var content = new byte[stream.Length];
            await stream.ReadAsync(content, 0, content.Length);
            return new NesFile(content);
        }
    }
}
