using System;
using System.IO;
using System.Drawing;
using LibEndianBinaryIO;
using LibNitro.GFX;
using LibNitro.Hardware;

namespace LibNitroG3DTools.Texture
{
    public class Texture
    {
        private readonly byte[] _palette;
        private readonly byte[] _texelData;
        private readonly byte[] _plttIdxData;

        public string Format { get; }
        public int Height { get; }
        public int Width { get; }
        public bool Color0Transparent { get; } = false;
        public string PaletteName { get; }
        public string TextureName { get; }

        public int BitmapSize => _texelData.Length / (Format == "tex4x4" ? 4 : 2);
        public int Tex4x4PaletteIndexSize => _plttIdxData.Length / 2;
        public int PaletteSize => _palette.Length / 2;

        public string BitmapData
        {
            get
            {
                var output = "";
                var er = new EndianBinaryReaderEx(new MemoryStream(_texelData), Endianness.LittleEndian);

                while (er.BaseStream.Position < er.BaseStream.Length)
                {
                    output += $"{(Format == "tex4x4" ? er.ReadUInt32().ToString("x8") : er.ReadUInt16().ToString("x4"))} ";
                }

                er.Close();

                return output;
            }
        }

        public string PaletteData
        {
            get
            {
                var output = "";
                var er = new EndianBinaryReaderEx(new MemoryStream(_palette), Endianness.LittleEndian);

                while (er.BaseStream.Position < er.BaseStream.Length)
                {
                    output += $"{er.ReadUInt16():x4} ";
                }

                er.Close();

                return output;
            }
        }

        public string Tex4x4PaletteIndexData
        {
            get
            {
                var output = "";
                var er = new EndianBinaryReaderEx(new MemoryStream(_plttIdxData), Endianness.LittleEndian);

                while (er.BaseStream.Position < er.BaseStream.Length)
                {
                    output += $"{er.ReadUInt16():x4} ";
                }

                er.Close();

                return output;
            }
        }

        public Texture(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Texture path is wrong or the file is missing: " + path);

            TextureName = Path.GetFileNameWithoutExtension(path);

            if (TextureName?.Length > 16)
                Console.WriteLine($"Warning: Texture Name Length of '{TextureName}' exceeds the 16 characters limit. It will be truncated upon binary conversion.");

            if (Path.GetExtension(path) == ".tga")
            {
                try
                {
                    var tga = new NitroTga(File.ReadAllBytes(path));

                    Color0Transparent = tga.NitroData.Color0Transparent;
                    Format = tga.NitroData.Format;
                    Height = tga.Header.ImageHeight;
                    Width = tga.Header.ImageWidth;

                    PaletteName = tga.NitroData.PlttName;
                    _palette = tga.NitroData.Palette;
                    _plttIdxData = tga.NitroData.PlttIdxData;
                    _texelData = tga.NitroData.TexelData;
                }
                catch
                {
                    throw new NotSupportedException("Non-Nitro TGAs are not supported: " + path);
                }
            }
            else if (Path.GetExtension(path) == ".png" || Path.GetExtension(path) == ".bmp")
            {
                try
                {
                    var bmp = new Bitmap(Image.FromFile(path));

                    /*if (TextureName.EndsWith("_cmp2") || TextureName.EndsWith("_cmp4"))
                    {
                        Textures.ConvertBitmap(bmp, out _texelData, out _palette, out _plttIdxData,
                            Textures.ImageFormat.COMP4x4, Textures.CharFormat.BMP, out var _);
                        Format = "tex4x4";
                    }
                    else if (bmp.Palette.Entries.Length <= 4)
                    {
                        Textures.ConvertBitmap(bmp, out _texelData, out _palette, out var _,
                            Textures.ImageFormat.PLTT4, Textures.CharFormat.BMP, out var col0);
                        Color0Transparent = col0;
                        Format = "palette4";
                    }
                    else if (bmp.Palette.Entries.Length <= 16)
                    {
                        Textures.ConvertBitmap(bmp, out _texelData, out _palette, out var _,
                            Textures.ImageFormat.PLTT16, Textures.CharFormat.BMP, out var col0);
                        Color0Transparent = col0;
                        Format = "palette16";
                    }
                    else if (bmp.Palette.Entries.Length <= 256)
                    {
                        Textures.ConvertBitmap(bmp, out _texelData, out _palette, out var _,
                            Textures.ImageFormat.PLTT256, Textures.CharFormat.BMP, out var col0);
                        Color0Transparent = col0;
                        Format = "palette256";
                    }
                    else
                    {
                        Textures.ConvertBitmap(bmp, out _texelData, out _palette, out var _,
                            Textures.ImageFormat.DIRECT, Textures.CharFormat.BMP, out var _);
                        Format = "direct";
                    }*/

                    //Format = "tex4x4";
                    //Textures.ConvertBitmap(bmp, out _texelData, out _palette, out _plttIdxData, Textures.ImageFormat.COMP4x4, Textures.CharFormat.CHAR, out var _);

                    Format = "palette256";
                    Textures.ConvertBitmap(bmp, out _texelData, out _palette, out _plttIdxData, Textures.ImageFormat.PLTT256, Textures.CharFormat.BMP, out var col0);
                    Color0Transparent = col0;

                    PaletteName = TextureName + "_pl";

                    Width = bmp.Width;
                    Height = bmp.Height;
                }
                catch
                {
                    throw new NotSupportedException($"Failed to convert the bitmap to '{Format}': " + path);
                }
            }
            else
            {
                throw new NotSupportedException("Image format not supported: " + path);
            }

            if (PaletteName?.Length > 16)
                Console.WriteLine($"Warning: Palette Name Length of '{PaletteName}' exceeds the 16 characters limit. It will be truncated upon binary conversion.");
        }
    }
}
