using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using LibEndianBinaryIO;
using LibFoundation.Math;
using LibNitro.GFX;

namespace LibNitro.Intermediate
{
    public static class Utils
    {
        public static ushort StringColorToXBGR(string c)
        {
            var rgbVals = new[]
            {
                byte.Parse(c.Split(' ')[0]) * 8,
                byte.Parse(c.Split(' ')[1]) * 8,
                byte.Parse(c.Split(' ')[2]) * 8
            };

            var color = (ushort)GFXUtil.ConvertColorFormat((uint)Color.FromArgb(rgbVals[0], rgbVals[1], rgbVals[2]).ToArgb(),
                ColorFormat.ARGB8888, ColorFormat.ABGR1555);

            return color;
        }
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