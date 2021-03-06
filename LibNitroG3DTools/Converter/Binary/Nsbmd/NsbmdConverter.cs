﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private static void DumpDisplayList(IEnumerable<DisplayListCommand> dl, string path)
        {
            File.WriteAllLines(path, dl.Select(x => $"{x}"));
        }

        private void Test()
        {
            var test = new NSBMD(File.ReadAllBytes(@"testfiles\patapata.nsbmd"));

            var dlA = test.ModelSet.models[0].shapes.shape[2].DL;
            var dlB = G3dDisplayList.Encode(G3dDisplayList.Decode(test.ModelSet.models[0].shapes.shape[2].DL).Where(x => x.G3dCommand != G3dCommand.Nop));

            var dlC = G3dDisplayList.Encode(new[]
            {
                new DisplayListCommand
                {
                    G3dCommand = G3dCommand.TexCoord, RealArgs = new[] {10f, 10f}
                },
                new DisplayListCommand
                {
                    G3dCommand = G3dCommand.Vertex, RealArgs = new[] {5f, 5f, 5f}
                },
                new DisplayListCommand
                {
                    G3dCommand = G3dCommand.End
                },
            });

            DumpDisplayList(G3dDisplayList.Decode(dlA), "testfiles/dlA.txt");
            DumpDisplayList(G3dDisplayList.Decode(dlB), "testfiles/dlB.txt");
            DumpDisplayList(G3dDisplayList.Decode(dlC), "testfiles/dlC.txt");

            //DL DECODE TEST
            /*var decoded = new List<List<DecodedCommand>>();

            foreach (var s in _testNsbmd.ModelSet.models[0].shapes.shape)
            {
                decoded.Add(G3dDisplayList.Decode(s.DL));
            }*/

            /*var dl = _testNsbmd.ModelSet.models[0].shapes.shape[0].DL;

            var decoded = G3dDisplayList.Decode(dl);
            var decodedNoNops = decoded.Where(x => x.G3dCommand != G3dCommand.Nop).ToList();
            var decodedImd = _imd.Body.PolygonArray.Polygons[0].MatrixPrimitives[0].PrimitiveArray.GetDecodedCommands();

            //var encoded = G3dDisplayList.Encode(decodedImd);

            var encodedFromNsbmd = G3dDisplayList.Encode(decoded); //Must be 992 bytes long
            var encodedNoNops = G3dDisplayList.Encode(decodedNoNops);

            var encodedFromImd = G3dDisplayList.Encode(decodedImd);
            var decoded3 = G3dDisplayList.Decode(encodedFromImd);

            var sbc = new Sbc.Sbc(_testNsbmd.ModelSet.models[0].sbc);

            var test = sbc.Write();

            _nsbmd.ModelSet.models[0].sbc = test;*/

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
            //File.WriteAllBytes(@"E:\ermel\Desktop\test.nsbmd", _nsbmd.Write());


            //var testNsbmd2 = new NSBMD(File.ReadAllBytes(@"testfiles/test.nsbmd"));

            //Test patching shapes
            /*for (int i = 0; i < _testNsbmd.ModelSet.models[0].shapes.shape.Length; i++)
            {
                var newUnwrittenShape = _nsbmd.ModelSet.models[0].shapes.shape[i];
                var newShape = testNsbmd2.ModelSet.models[0].shapes.shape[i];
                var oldShape = _testNsbmd.ModelSet.models[0].shapes.shape[i];

                DumpDisplayList(G3dDisplayList.Decode(newUnwrittenShape.DL), $"testfiles/newdl/new_{i}.txt");
                DumpDisplayList(G3dDisplayList.Decode(oldShape.DL), $"testfiles/olddl/{i}.txt");

                File.AppendAllText("testfiles/newdl/dlinfo.txt", $"NEW SHAPE: Encoded DL Length = {newUnwrittenShape.DL.Length} ; Decoded DL Length = {G3dDisplayList.Decode(newUnwrittenShape.DL).Count}" +
                                                                 $" OLD SHAPE: Encoded DL Length = {oldShape.DL.Length} ; Decoded DL Length = {G3dDisplayList.Decode(oldShape.DL).Count}\n");
            }*/
        }

        public NsbmdConverter(string path)
        {
            _imd = Imd.Read(path);

            if (!ValidateImd(_imd))
                throw new InvalidDataException("IMD file has errors inside.");

            _imdName = Path.GetFileNameWithoutExtension(path);

            _nsbmd = new NSBMD(false);
            
            GetModelSet();
            GetTextureSet();
            
            //File.WriteAllBytes("testfiles/test.nsbmd",_nsbmd.Write());
        }

        public void Write(string path)
        {
            File.WriteAllBytes(path, _nsbmd.Write());

            var texPath = Path.Combine(Path.GetDirectoryName(path.Replace("\"", String.Empty)),
                Path.GetFileNameWithoutExtension(path) + ".nsbtx");

            if (_nsbtx != null)
                File.WriteAllBytes(texPath, _nsbtx.Write());
        }

        private static bool ValidateImd(Imd imd)
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
