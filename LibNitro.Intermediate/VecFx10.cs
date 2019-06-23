
using System;
using System.IO;
using LibEndianBinaryIO;
using LibFoundation.Math;

namespace LibNitro.Intermediate
{
    //Very gay tbh

    public class VecFx10
    {
        public static uint FromVector3(Vector3 vec, int intPart = 0, int fracPart = 9)
        {
            var buffer = new byte[] {0x00, 0x00};

            var coords = new ushort[3];

            var m = new MemoryStream(buffer);

            var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);

            ew.WriteFixedPoint(vec.X, true, intPart, fracPart);
            m.Position = 0;
            coords[0] = er.ReadUInt16();

            m.Position = 0;
            ew.WriteFixedPoint(vec.Y, true, intPart, fracPart);
            m.Position = 0;
            coords[1] = er.ReadUInt16();

            m.Position = 0;
            ew.WriteFixedPoint(vec.Z, true, intPart, fracPart);
            m.Position = 0;
            coords[2] = er.ReadUInt16();

            m.Close();

            var res = (uint)(coords[0] | (coords[1] << 10) | (coords[2] << 20));

            return res;
        }

        public static Vector3 ToVector3(uint vec, int intPart = 0, int fracPart = 9)
        {
            var buffer = new byte[] { 0x00, 0x00 };
            var coords = new[] {(ushort) (vec & 0x3FF), (ushort) ((vec >> 10) & 0x3FF), (ushort) ((vec >> 20) & 0x3FF)};

            var m = new MemoryStream(buffer);

            var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);

            ew.Write(coords[0]);
            m.Position = 0;
            float x = er.ReadFixedPoint(true, intPart, fracPart);
            m.Position = 0;
            ew.Write(coords[1]);
            m.Position = 0;
            float y = er.ReadFixedPoint(true, intPart, fracPart);
            m.Position = 0;
            ew.Write(coords[2]);
            m.Position = 0;
            float z = er.ReadFixedPoint(true, intPart, fracPart);

            m.Close();

            return (x, y, z);
        }
    }
}
