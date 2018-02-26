using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;

namespace LoveNes.IO
{
    public struct SpanReader
    {
        private ReadOnlySpan<byte> _span;

        public bool IsCosumed => _span.IsEmpty;

        public SpanReader ReadAsSubReader(int length)
        {
            var reader = new SpanReader(_span.Slice(0, length));
            Advance(length);
            return reader;
        }

        public SpanReader(ReadOnlySpan<byte> span)
        {
            _span = span;
        }

        public ushort ReadAsUnsignedShort()
        {
            var value = _span.ReadBigEndian<ushort>();
            Advance(sizeof(ushort));
            return value;
        }

        public uint ReadAsUnsignedInt()
        {
            var value = _span.ReadBigEndian<uint>();
            Advance(sizeof(uint));
            return value;
        }

        public ulong ReadAsUnsignedLong()
        {
            var value = _span.ReadBigEndian<ulong>();
            Advance(sizeof(ulong));
            return value;
        }

        public int ReadAsInt()
        {
            var value = _span.ReadBigEndian<int>();
            Advance(sizeof(int));
            return value;
        }

        public long ReadAsLong()
        {
            var value = _span.ReadBigEndian<long>();
            Advance(sizeof(long));
            return value;
        }

        public byte PeekAsByte()
        {
            var value = _span.ReadBigEndian<byte>();
            return value;
        }

        public byte ReadAsByte()
        {
            var value = _span.ReadBigEndian<byte>();
            Advance(sizeof(byte));
            return value;
        }

        public bool ReadAsBoolean()
        {
            var value = _span.ReadBigEndian<bool>();
            Advance(sizeof(bool));
            return value;
        }

        public short ReadAsShort()
        {
            var value = _span.ReadBigEndian<short>();
            Advance(sizeof(short));
            return value;
        }

        public float ReadAsFloat()
        {
            var value = _span.ReadBigEndian<float>();
            Advance(sizeof(float));
            return value;
        }

        public double ReadAsDouble()
        {
            var value = _span.ReadBigEndian<double>();
            Advance(sizeof(double));
            return value;
        }

        public byte[] ReadAsByteArray(int length)
        {
            var value = ReadBytes(length);
            return value.ToArray();
        }

        public ReadOnlySpan<byte> ReadAsSpan(int length)
        {
            return ReadBytes(length);
        }

        public byte[] ReadAsByteArray()
        {
            var bytes = _span.ToArray();
            _span = ReadOnlySpan<byte>.Empty;
            return bytes;
        }

        private ReadOnlySpan<byte> ReadBytes(int length)
        {
            var bytes = _span.Slice(0, length);
            Advance(length);
            return bytes;
        }

        public void Advance(int count)
        {
            _span = _span.Slice(count);
        }
    }
}
