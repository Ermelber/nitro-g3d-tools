using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using LibEndianBinaryIO;

namespace LibNitro.Intermediate
{
    public static class Utils
    {
        public static byte[] StringDataToBytes(string text)
        {
            var split = Regex.Replace(Regex.Replace(text, @"(\\.)", " "), @"\s+", " ").Trim().Split(' ');

            var m = new MemoryStream();
            var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);

            foreach (var value in split)
            {
                switch (value.Length)
                {
                    case 2:
                        ew.Write(byte.Parse(value,NumberStyles.HexNumber));
                        break;
                    case 4:
                        ew.Write(ushort.Parse(value, NumberStyles.HexNumber));
                        break;
                    case 8:
                        ew.Write(uint.Parse(value, NumberStyles.HexNumber));
                        break;
                }
            }

            var bytes = m.ToArray();
            m.Close();

            return bytes;
        }
    }
}