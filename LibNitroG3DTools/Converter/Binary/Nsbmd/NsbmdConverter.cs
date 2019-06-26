using System;
using System.Collections.Generic;
using System.IO;
using LibNitro.G3D.BinRes;
using LibNitro.Intermediate.Imd;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd
{
    public class NsbmdConverter
    {
        private NSBMD _nsbmd;
        private NSBTX _nsbtx = null;
        private Imd _imd;
        private string _imdName;

        private NSBMD _testNsbmd;
        private NSBTX _testNsbtx;

        public NsbmdConverter(string path)
        {
            _imd = Imd.Read(path);

            if (!ImdChecker(_imd))
                throw new InvalidDataException("IMD file has errors inside.");

            _imdName = Path.GetFileNameWithoutExtension(path);

            _nsbmd = new NSBMD(false);

            //_testNsbmd = new NSBMD(File.ReadAllBytes(@"E:\ermel\Hackdom\DSHack\EKDS\GRAPHICS\GRAPHICS\Models\Tracks\Retro\MK8 Animal Crossing\imd\mori_spring.nsbmd"));
            //_testNsbtx = new NSBTX(File.ReadAllBytes(@"E:\ermel\Hackdom\DSHack\EKDS\GRAPHICS\GRAPHICS\Models\Tracks\Retro\MK8 Animal Crossing\imd\mori_spring.nsbtx"));

            //_testNsbmd = new NSBMD(File.ReadAllBytes(@"E:\ermel\Hackdom\DSHack\EKDS\GRAPHICS\GRAPHICS\workspace\P_PC.nsbmd"));


            _testNsbmd = new NSBMD(File.ReadAllBytes(@"testfiles\patapata.nsbmd"));

            GetModelSet();
            //GetTextureSet();

            //File.WriteAllBytes(@"E:\ermel\Desktop\test.nsbmd", _nsbmd.Write());

            //DL DECODE TEST
            /*var decoded = new List<List<DecodedCommand>>();

            foreach (var s in _testNsbmd.ModelSet.models[0].shapes.shape)
            {
                decoded.Add(G3dDisplayList.Decode(s.DL));
            }*/

            var dl = _testNsbmd.ModelSet.models[0].shapes.shape[0].DL;

            var decoded = G3dDisplayList.Decode(dl);
            var decodedImd = _imd.Body.PolygonArray.Polygons[0].MatrixPrimitives[0].PrimitiveArray.GetDecodedCommands();

            //var encoded = G3dDisplayList.Encode(decodedImd);

            var encoded = G3dDisplayList.Encode(decoded);
            var encoded2 = G3dDisplayList.Encode(decodedImd);

            /*for (int i = 0; i < dl.Length; i++)
            {
                try
                {
                    if (dl[i] != encoded[i])
                    {
                        Console.WriteLine($"Different! At offset {i}: {dl[i]} {encoded[i]}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }*/

            File.WriteAllBytes("testfiles/test.nsbmd",_nsbmd.Write());
        }

        public void Write(string path)
        {
            var texPath = Path.Combine(Path.GetDirectoryName(path.Replace("\"", String.Empty)),
                Path.GetFileNameWithoutExtension(path) + ".nsbtx");

            if (_nsbtx != null)
                File.WriteAllBytes(texPath, _nsbtx.Write());
        }

        private static bool ImdChecker(Imd imd)
        {
            var valid = true;

            //Textures
            //Check texture names clashes
            var texes = new List<string>();

            foreach (var tex in imd.Body.TexImageArray.TexImages)
            {
                var name = tex.Name.Length > 16 ? tex.Name.Substring(0, 16) : tex.Name;

                if (!texes.Contains(name))
                {
                    texes.Add(name);
                }
                else
                {
                    Console.Write($"Error: There are multiple textures with the same name '{name}'");
                    valid = false;
                }
            }

            //Palettes
            //Check palette names clashes
            var pals = new List<string>();

            foreach (var pal in imd.Body.TexPaletteArray.TexPalettes)
            {
                var name = pal.Name.Length > 16 ? pal.Name.Substring(0, 16) : pal.Name;

                if (!pals.Contains(name))
                {
                    pals.Add(name);
                }
                else
                {
                    Console.Write($"Error: There are multiple palettes with the same name '{name}'");
                    valid = false;
                }
            }

            return valid;
        }

        private void GetModelSet()
        {
            _nsbmd.ModelSet.dict = new Dictionary<MDL0.MDL0Data>();
            _nsbmd.ModelSet.dict.Add(_imdName, new MDL0.MDL0Data());
            _nsbmd.ModelSet.models = new[] { MdlUtils.GetModel(_imd) };
        }

        private void GetTextureSet(bool nsbtx = true)
        {
            if (nsbtx)
            {
                _nsbtx = new NSBTX();

                _nsbtx.TexPlttSet = TexUtils.GetTextures(_imd);
            }
        }

    }
}
