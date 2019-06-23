using LibNitro.G3D.BinRes;
using LibNitro.Hardware;
using LibNitro.Intermediate.Imd;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd
{
    public static class TexUtils
    {
        public static TEX0 GetTextures(Imd imd)
        {
            var tex = new TEX0
            {
                TexInfo = new TEX0.texInfo(),
                Tex4x4Info = new TEX0.tex4x4Info(),
                PlttInfo = new TEX0.plttInfo(),
                dictTex = GetDictTexData(imd),
                dictPltt = GetDictPlttData(imd)
            };

            return tex;
        }

        private static Dictionary<TEX0.DictTexData> GetDictTexData(Imd imd)
        {
            var dict = new Dictionary<TEX0.DictTexData>();

            foreach (var tex in imd.Body.TexImageArray.TexImages)
            {
                var imgFormat = Textures.ImageFormat.NONE;
                switch (tex.Format)
                {
                    case "tex4x4":
                        imgFormat = Textures.ImageFormat.COMP4x4;
                        break;
                    case "palette4":
                        imgFormat = Textures.ImageFormat.PLTT4;
                        break;
                    case "palette16":
                        imgFormat = Textures.ImageFormat.PLTT16;
                        break;
                    case "palette256":
                        imgFormat = Textures.ImageFormat.PLTT256;
                        break;
                    case "direct":
                        imgFormat = Textures.ImageFormat.DIRECT;
                        break;
                    case "a3i5":
                        imgFormat = Textures.ImageFormat.A3I5;
                        break;
                    case "a5i3":
                        imgFormat = Textures.ImageFormat.A5I3;
                        break;
                }

                var data = new TEX0.DictTexData
                {
                    Data = tex.Bitmap.Bytes,
                    Data4x4 = tex.Tex4x4PaletteIndex?.Bytes,
                    Fmt = imgFormat,
                    S = (ushort)tex.Width,
                    T = (ushort)tex.Height,
                    TransparentColor = tex.Color0Mode != null && tex.Color0Mode == "transparency"
                };


                dict.Add(tex.Name.Length > 16 ? tex.Name.Substring(0, 16) : tex.Name, data);
            }

            return dict;
        }

        private static Dictionary<TEX0.DictPlttData> GetDictPlttData(Imd imd)
        {
            var dict = new Dictionary<TEX0.DictPlttData>();

            foreach (var plt in imd.Body.TexPaletteArray.TexPalettes)
            {
                var data = new TEX0.DictPlttData
                {
                    Data = plt.Bytes
                };

                dict.Add(plt.Name.Length > 16 ? plt.Name.Substring(0, 16) : plt.Name, data);
            }

            return dict;
        }
    }
}
