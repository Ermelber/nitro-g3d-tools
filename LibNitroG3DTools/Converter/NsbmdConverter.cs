using System;
using System.Collections.Generic;
using System.IO;
using LibNitro.G3D.BinRes;
using LibNitro.Hardware;
using LibNitro.Intermediate;

namespace LibNitroG3DTools.Converter
{
    public class NsbmdConverter
    {
        private NSBMD _nsbmd;
        private NSBTX _nsbtx = null;
        private Imd _imd;

        private NSBMD _testNsbmd;
        private NSBTX _testNsbtx;

        public NsbmdConverter(string path)
        {
            _imd = Imd.Read(path);

            if (!ImdChecker(_imd))
                throw new InvalidDataException("IMD file has errors inside.");

            _nsbmd = new NSBMD(false);

            _testNsbmd = new NSBMD(File.ReadAllBytes(@"E:\ermel\Hackdom\DSHack\EKDS\GRAPHICS\GRAPHICS\Models\Tracks\Retro\Wii Rainbow Road\Wii RR\intermediate\course_model.nsbmd"));
            _testNsbtx = new NSBTX(File.ReadAllBytes(@"E:\ermel\Hackdom\DSHack\EKDS\GRAPHICS\GRAPHICS\Models\Tracks\Retro\Wii Rainbow Road\Wii RR\intermediate\course_model.nsbtx"));

            GetModelSet();
            GetTexturesSet();

            //File.WriteAllBytes(@"E:\ermel\Desktop\test.nsbmd", _nsbmd.Write());
            
        }

        public void Write(string path)
        {
            var texPath = Path.Combine(Path.GetDirectoryName(path.Replace("\"", String.Empty)),
                Path.GetFileNameWithoutExtension(path) + ".nsbtx");

            if (_nsbtx != null)
                File.WriteAllBytes(texPath, _nsbtx.Write());
        }

        private bool ImdChecker(Imd imd)
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

        private MDL0.Model.ModelInfo GetModelInfo(Imd imd)
        {
            return new MDL0.Model.ModelInfo
            {
                //BoxTest
                boxX = imd.Body.BoxTest.Position.X,
                boxY = imd.Body.BoxTest.Position.Y,
                boxZ = imd.Body.BoxTest.Position.Z,

                boxW = imd.Body.BoxTest.Size.X,
                boxH = imd.Body.BoxTest.Size.Y,
                boxD = imd.Body.BoxTest.Size.Z,

                boxPosScale = 1 << imd.Body.BoxTest.PosScale,
                boxInvPosScale = 1f / (1 << imd.Body.BoxTest.PosScale),

                //ModelInfo
                firstUnusedMtxStackID = 1, //todo: Use real value

                posScale = 1 << imd.Body.ModelInfo.PosScale,
                invPosScale = 1f / (1 << imd.Body.ModelInfo.PosScale),

                numMat = imd.Body.ModelInfo.MaterialCount,
                numNode = imd.Body.ModelInfo.NodeCount,
                numShp = (byte)imd.Body.PolygonArray.Size,
                numPolygon = (ushort)imd.Body.OutputInfo.PolygonSize,
                numQuad = (ushort)imd.Body.OutputInfo.QuadSize,
                numTriangle = (ushort)imd.Body.OutputInfo.TriangleSize,
                numVertex = (ushort)imd.Body.OutputInfo.VertexSize,

                sbcType = 0, //todo: Use real value
                scalingRule = 0, //todo: Use real value
                texMtxMode = 0 //todo: Use real value
            };
        }

        private MDL0.Model GetModel(Imd imd)
        {
            var model = new MDL0.Model
            {
                evpMatrices = null, //todo: Use real value

                info = GetModelInfo(imd)
            };

            return model;
        }

        private void GetModelSet()
        {
            _nsbmd.ModelSet.models = new MDL0.Model[1] { GetModel(_imd) };
        }

        private Dictionary<TEX0.DictTexData> GetDictTexData(Imd imd)
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
                    S = (ushort) tex.Width,
                    T = (ushort) tex.Height,
                    TransparentColor = tex.Color0Mode != null && tex.Color0Mode == "transparency"
                };
                

                dict.Add(tex.Name.Length > 16 ? tex.Name.Substring(0, 16) : tex.Name, data);
            }

            return dict;
        }

        private Dictionary<TEX0.DictPlttData> GetDictPlttData(Imd imd)
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

        private TEX0 GetTextures(Imd imd)
        {
            var tex = new TEX0();

            tex.TexInfo = new TEX0.texInfo();
            tex.Tex4x4Info = new TEX0.tex4x4Info();
            tex.PlttInfo = new TEX0.plttInfo();

            tex.dictTex = GetDictTexData(imd);
            tex.dictPltt = GetDictPlttData(imd);
            
            return tex;
        }

        private void GetTexturesSet(bool nsbtx = true)
        {
            if (nsbtx)
            {
                _nsbtx = new NSBTX();

                _nsbtx.TexPlttSet = GetTextures(_imd);
            }
        }

    }
}
