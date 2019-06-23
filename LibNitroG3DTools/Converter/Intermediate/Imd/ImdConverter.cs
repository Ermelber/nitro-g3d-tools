using System;
using System.Collections.Generic;
using System.IO;
using Assimp;
using LibFoundation.Math;
using LibNitro.Intermediate;
using LibNitro.Intermediate.Imd;
using Material = LibNitro.Intermediate.Imd.Material;
using Node = LibNitro.Intermediate.Imd.Node;

namespace LibNitroG3DTools.Converter.Intermediate.Imd
{
    public class ImdConverter
    {
        public static readonly GeneratorInfo GeneratorInfo = new GeneratorInfo
        {
            Name = "ASS to IMD (Made by Ermelber)",
            Version = "0.2.5"
        };

        private readonly ImdConverterSettings _settings;
        private readonly Scene _scene;
        private ModelBounds _bounds;

        ///Key: Mesh Id, Value: List of vertices
        private Dictionary<int, List<Vector3>> _meshes = new Dictionary<int, List<Vector3>>();
        private readonly string _modelDirectory;

        private LibNitro.Intermediate.Imd.Imd _imd;

        private void GetPosScales()
        {
            _bounds = ModelBounds.Calculate(_meshes);
            _imd.Body.ModelInfo.PosScale = _bounds.GetPosScale();
            _imd.Body.BoxTest.PosScale = _bounds.GetBoxPosScale();

            Vector3 xyz =
                (((int)Math.Round(_bounds.BoxXyz.X * 4096) >> _imd.Body.BoxTest.PosScale) / 4096f,
                ((int)Math.Round(_bounds.BoxXyz.Y * 4096) >> _imd.Body.BoxTest.PosScale) / 4096f,
                ((int)Math.Round(_bounds.BoxXyz.Z * 4096) >> _imd.Body.BoxTest.PosScale) / 4096f);

            Vector3 whd =
                (((int)Math.Round(_bounds.BoxWhd.X * 4096) >> _imd.Body.BoxTest.PosScale) / 4096f,
                ((int)Math.Round(_bounds.BoxWhd.Y * 4096) >> _imd.Body.BoxTest.PosScale) / 4096f,
                ((int)Math.Round(_bounds.BoxWhd.Z * 4096) >> _imd.Body.BoxTest.PosScale) / 4096f);

            //_imd.Body.BoxTest.Xyz = $"{xyz.X} {xyz.Y} {xyz.Z}";
            //_imd.Body.BoxTest.Whd = $"{whd.X} {whd.Y} {whd.Z}";

            _imd.Body.BoxTest.Position = xyz;
            _imd.Body.BoxTest.Size = whd;
        }

        private void GetTextures()
        {
            var palettes = new List<TexPalette>();
            var textures = new List<TexImage>();

            int texId = 0;
            int paletteId = 0;

            if (_scene.HasTextures)
            {
                throw new NotImplementedException("Embedded Textures aren't implemented yet");
                /*foreach (var tex in scene.Textures)
                {
                    
                }*/
            }
            else
            {
                foreach (var mat in _scene.Materials)
                {
                    if (mat.HasTextureDiffuse)
                    {
                        var tex = mat.GetMaterialTextures(TextureType.Diffuse)[0];

                        if (_imd.Body.TexImageArray == null)
                        {
                            _imd.Body.TexImageArray = new TexImageArray();
                        }

                        var texPath = Path.GetFullPath(Path.Combine(_modelDirectory,
                            tex.FilePath.Substring(0, 2) == "//" ? tex.FilePath.Remove(0, 2) : tex.FilePath));
                        var texName = Path.GetFileNameWithoutExtension(texPath);
                        

                        //Prevents doubled textures
                        bool repeated = false;
                        foreach (var prevTex in textures)
                        {
                            if (prevTex.Name == texName)
                            {
                                repeated = true;
                                break;
                            }
                        }
                        if (repeated) continue;

                        var texture = new Texture.Texture(texPath);

                        if (_imd.Body.TexPaletteArray == null && texture.Format != "direct")
                        {
                            _imd.Body.TexPaletteArray = new TexPaletteArray();
                        }

                        var texImage = new TexImage
                        {
                            Index = texId++,
                            Name = texture.TextureName,
                            Path = texPath,
                            Height = texture.Height,
                            Width = texture.Width,
                            Bitmap = new TexBitmap
                            {
                                Size = texture.BitmapSize,
                                Value = texture.BitmapData
                            },
                            Format = texture.Format,
                            OriginalHeight = texture.Height,
                            OriginalWidth = texture.Width,
                            PaletteName = texture.PaletteName
                        };

                        if (texture.Format == "tex4x4")
                        {
                            texImage.Tex4x4PaletteIndex = new Tex4x4PaletteIndex
                            {
                                Size = texture.Tex4x4PaletteIndexSize,
                                Value = texture.Tex4x4PaletteIndexData
                            };
                        }
                        else if (texture.Format == "palette4" || texture.Format == "palette16" ||
                                 texture.Format == "palette256")
                        {
                            texImage.Color0Mode = texture.Color0Transparent ? "transparency" : "color";
                        }

                        textures.Add(texImage);

                        if (texture.Format != "direct")
                        {
                            palettes.Add(new TexPalette
                            {
                                ColorSize = texture.PaletteSize,
                                Index = paletteId++,
                                Name = texture.PaletteName,
                                Value = texture.PaletteData
                            });
                        }
                    }
                }
            }

            if (_imd.Body.TexImageArray != null)
            {
                _imd.Body.TexImageArray.TexImages = textures;

                if (_imd.Body.TexPaletteArray != null)
                    _imd.Body.TexPaletteArray.TexPalettes = palettes;
            }
        }

        private void GetMaterials()
        {
            int i = 0;
            foreach (var mat in _scene.Materials)
            {
                var material = new Material
                {
                    Index = i,
                    Name = mat.Name.Length > 16 ? $"{mat.Name.Substring(0, 13)}{i}" : mat.Name, //Fixes the length of the material name
                    Alpha = (byte)(mat.Opacity * 31),
                    Ambient = $"{(int)(mat.ColorAmbient.R * 31)} {(int)(mat.ColorAmbient.G * 31)} {(int)(mat.ColorAmbient.B * 31)}",
                    Diffuse = $"{(int)(mat.ColorDiffuse.R * 31)} {(int)(mat.ColorDiffuse.G * 31)} {(int)(mat.ColorDiffuse.B * 31)}",
                    Emission = $"{(int)(mat.ColorEmissive.R * 31)} {(int)(mat.ColorEmissive.G * 31)} {(int)(mat.ColorEmissive.B * 31)}",
                    Specular = $"{(int)(mat.ColorSpecular.R * 31)} {(int)(mat.ColorSpecular.G * 31)} {(int)(mat.ColorSpecular.B * 31)}",
                    Light0 = _settings.NoLightOnMaterials ? "off" : "on"
                };

                if (mat.HasTextureDiffuse)
                {
                    material.TexImageIdx =
                        _imd.Body.TexImageArray.GetIndexByName(
                            Path.GetFileNameWithoutExtension(mat.TextureDiffuse.FilePath));

                    var curTex = _imd.Body.TexImageArray.TexImages[material.TexImageIdx];

                    if (curTex.Format != "direct")
                    {
                        material.TexPaletteIdx = _imd.Body.TexPaletteArray.GetIndexByName(curTex.PaletteName);
                    }

                    material.TexTiling = mat.TextureDiffuse.WrapModeU == TextureWrapMode.Mirror
                        ? "flip "
                        : mat.TextureDiffuse.WrapModeU == TextureWrapMode.Clamp
                            ? "clamp "
                            : "repeat ";
                    material.TexTiling += mat.TextureDiffuse.WrapModeV == TextureWrapMode.Mirror
                        ? "flip"
                        : mat.TextureDiffuse.WrapModeV == TextureWrapMode.Clamp
                            ? "clamp"
                            : "repeat";

                    material.TexScale = "1.0000 1.0000";
                    material.TexRotate = "0.0000";
                    material.TexTranslate = "0.0000 0.0000";

                    material.TexGenMode = "none";
                }

                _imd.Body.MaterialArray.Materials.Add(material);

                i++;
            }

            _imd.Body.ModelInfo.MaterialSize = $"{_scene.MaterialCount} {_scene.MaterialCount}";
        }

        private void GetMatrices()
        {
            _imd.Body.MatrixArray.Matrices.Add(new Matrix
            {
                Index = 0,
                MatrixWeight = 1,
                NodeIndex = 0
            });
        }

        private void GetPolygons()
        {
            if (_settings.UsePrimitiveStrip) throw new NotSupportedException("Primitive Strip isn't supported yet");

            var posScale = _imd.Body.ModelInfo.PosScale;
            var rootNode = _imd.Body.NodeArray.Nodes[0];

            var polygonId = 0;
            foreach (var mesh in _meshes)
            {
                var meshId = mesh.Key;
                var sceneMesh = _scene.Meshes[meshId];
                var materialId = sceneMesh.MaterialIndex;
                var material = _imd.Body.MaterialArray.Materials[materialId];
                var useNormals = material.Light0 == "on" || material.Light1 == "on" || material.Light2 == "on" || material.Light3 == "on";

                var texture = material.TexImageIdx != -1
                    ? _imd.Body.TexImageArray.TexImages[material.TexImageIdx]
                    : null;

                var polygon = new Polygon
                {
                    Index = polygonId,
                    Name = $"polygon{polygonId}",
                };

                var matrixPrimitive = new MatrixPrimitive
                {
                    Index = 0,
                    PrimitiveArray = new PrimitiveArray
                    {
                        Primitives = new List<Primitive>()
                    }
                };
                polygon.MatrixPrimitives.Add(matrixPrimitive);

                //Previous values
                var col = new Color4D(1, 1, 1);
                int prevX = 0;
                int prevY = 0;
                int prevZ = 0;

                Primitive quadPrimitive = new Primitive { Type = "quads" };
                Primitive triPrimitive = new Primitive { Type = "triangles" };

                var primitive = triPrimitive;
                int prevIndexCount = 0;

                foreach (var face in sceneMesh.Faces)
                {
                    if (face.IndexCount == 3)
                    {
                        primitive = triPrimitive;
                    }
                    else if (face.IndexCount == 4)
                    {
                        primitive = quadPrimitive;
                    }
                    else continue; //Other shapes????!

                    foreach (var vertexIndex in face.Indices)
                    {
                        var vertex = _meshes[meshId][vertexIndex];

                        //Vertex Colors
                        if (sceneMesh.HasVertexColors(0) && (col == new Color4D(1, 1, 1) || col != sceneMesh.VertexColorChannels[0][vertexIndex]))
                        {
                            col = sceneMesh.VertexColorChannels[0][vertexIndex];
                            primitive.Commands.Add(new ColorCommand(col.R, col.G, col.B));
                        }

                        //Texture Coordinates
                        if (texture != null)
                        {
                            var texCoord = sceneMesh.TextureCoordinateChannels[0][vertexIndex];
                            primitive.Commands.Add(new TextureCoordCommand(texCoord.X, texCoord.Y, texture.Width, texture.Height));
                        }

                        //Normals
                        if (useNormals)
                        {
                            var normal = sceneMesh.Normals[vertexIndex];
                            primitive.Commands.Add(new NormalCommand(new Vector3(normal.X, normal.Y, normal.Z)));
                        }

                        //Vertex
                        var x = (int)Math.Round(vertex.X * 4096) >> posScale;
                        var y = (int)Math.Round(vertex.Y * 4096) >> posScale;
                        var z = (int)Math.Round(vertex.Z * 4096) >> posScale;
                        var scaledVtx = (x / 4096f, y / 4096f, z / 4096f);

                        var diffX = x - prevX; var diffY = y - prevY; var diffZ = z - prevZ;
                        var diff = (diffX / 4096f, diffY / 4096f, diffZ / 4096f);

                        if (vertexIndex != 0 && prevIndexCount == face.IndexCount && diffX == 0)
                        {
                            primitive.Commands.Add(new PosYzCommand(scaledVtx));
                        }
                        else if (vertexIndex != 0 && prevIndexCount == face.IndexCount && diffY == 0)
                        {
                            primitive.Commands.Add(new PosXzCommand(scaledVtx));
                        }
                        else if (vertexIndex != 0 && prevIndexCount == face.IndexCount && diffZ == 0)
                        {
                            primitive.Commands.Add(new PosXyCommand(scaledVtx));
                        }
                        else if (vertexIndex != 0 && prevIndexCount == face.IndexCount &&
                                    diffX < 512 && diffX > -512 &&
                                    diffY < 512 && diffY > -512 &&
                                    diffZ < 512 && diffZ > -512)
                        {
                            primitive.Commands.Add(new PosDiffCommand(diff));
                        }
                        else if ((x & 0x3F) == 0 && (y & 0x3F) == 0 && (z & 0x3F) == 0)
                        {
                            primitive.Commands.Add(new PosShortCommand(scaledVtx));
                        }
                        else
                        {
                            primitive.Commands.Add(new PosXyzCommand(scaledVtx));
                        }

                        prevX = x;
                        prevY = y;
                        prevZ = z;

                        polygon.VertexSize++;
                        primitive.VertexSize++;
                    }


                    polygon.PolygonSize++;

                    if (face.IndexCount == 3)
                    {
                        polygon.TriangleSize++;
                    }
                    else if (face.IndexCount == 4)
                    {
                        polygon.QuadSize++;
                    }

                    prevIndexCount = face.IndexCount;
                }

                if (polygon.VertexSize == 0) continue;

                if (polygon.QuadSize != 0)
                {
                    quadPrimitive.Commands.Insert(0, new MatrixCommand { Index = 0 });

                    quadPrimitive.Index = matrixPrimitive.PrimitiveArray.Primitives.Count;
                    matrixPrimitive.PrimitiveArray.Primitives.Add(quadPrimitive);
                }
                if (polygon.TriangleSize != 0)
                {
                    if (polygon.QuadSize == 0) triPrimitive.Commands.Insert(0, new MatrixCommand { Index = 0 });

                    triPrimitive.Index = matrixPrimitive.PrimitiveArray.Primitives.Count;
                    matrixPrimitive.PrimitiveArray.Primitives.Add(triPrimitive);
                }


                _imd.Body.PolygonArray.Polygons.Add(polygon);
                _imd.Body.OutputInfo.PolygonSize += polygon.PolygonSize;
                _imd.Body.OutputInfo.TriangleSize += polygon.TriangleSize;
                _imd.Body.OutputInfo.QuadSize += polygon.QuadSize;
                _imd.Body.OutputInfo.VertexSize += polygon.VertexSize;

                if (_imd.Body.ModelInfo.CompressNode == "unite_combine")
                {
                    rootNode.PolygonSize += polygon.PolygonSize;
                    rootNode.TriangleSize += polygon.TriangleSize;
                    rootNode.QuadSize += polygon.QuadSize;
                    rootNode.VertexSize += polygon.VertexSize;

                    rootNode.Displays.Add(new NodeDisplay
                    {
                        Index = rootNode.Displays.Count,
                        Polygon = polygonId,
                        Material = materialId,
                        Priority = 0
                    });
                }
                else
                {
                    throw new NotSupportedException("Only Compress Node Mode \"unite_combine\" is supported for now");
                }

                polygonId++;
            }
        }

        private void AddTransformedVertices(Assimp.Node node)
        {
            var parent = node;
            var transform = _settings.RotateX180 ? Matrix44.CreateRotationX(180) : Matrix44.Identity;
            while (parent != null)
            {
                transform = new Matrix44(
                    parent.Transform.A1, parent.Transform.A2, parent.Transform.A3, parent.Transform.D1,
                    parent.Transform.B1, parent.Transform.B2, parent.Transform.B3, parent.Transform.D2,
                    parent.Transform.C1, parent.Transform.C2, parent.Transform.C3, parent.Transform.D3,
                    parent.Transform.A4, parent.Transform.B4, parent.Transform.C4, parent.Transform.D4
                ) * transform;

                parent = parent.Parent;
            }

            foreach (var meshId in node.MeshIndices)
            {
                var mesh = _scene.Meshes[meshId];

                _meshes.Add(meshId, new List<Vector3>());

                foreach (var originalVtx in mesh.Vertices)
                {
                    var vtx = new Vector3(originalVtx.X, originalVtx.Y, originalVtx.Z) * transform * _imd.Body.ModelInfo.Magnify;

                    if (_settings.FlipYZ)
                        vtx = (vtx.X, vtx.Z, vtx.Y);

                    //FX32 conversion
                    vtx = ((float)Math.Round(vtx.X * 4096) / 4096f, (float)Math.Round(vtx.Y * 4096) / 4096f, (float)Math.Round(vtx.Z * 4096) / 4096f);

                    _meshes[meshId].Add(vtx);
                }
            }
        }

        //Support for unite combine only
        private void GetNodes(Assimp.Node node)
        {
            if (_imd.Body.ModelInfo.CompressNode != "unite_combine") throw new NotSupportedException("Only Compress Node Mode \"unite_combine\" is supported for now");

            //The first time adds the root node
            if (_imd.Body.NodeArray.Nodes.Count == 0 && _imd.Body.ModelInfo.CompressNode == "unite_combine")
            {
                _imd.Body.ModelInfo.NodeSize = "1 1";

                _imd.Body.NodeArray.Nodes.Add(new Node
                {
                    Index = 0,
                    Name = "world_root",
                    Kind = "mesh"
                });
            }

            if (node.HasMeshes)
            {
                AddTransformedVertices(node);
            }

            foreach (var children in node.Children)
            {
                GetNodes(children);
            }
        }

        public ImdConverter(string path, ImdConverterSettings settings = null)
        {
            _settings = settings ?? new ImdConverterSettings();

            var context = new AssimpContext();
            _scene = context.ImportFile(path);

            _imd = new LibNitro.Intermediate.Imd.Imd
            {
                Head = { GeneratorInfo = GeneratorInfo },
                Body = { OriginalGeneratorInfo = GeneratorInfo }
            };

            _modelDirectory = Path.GetDirectoryName(path);
            _imd.Head.CreateInfo.Source = Path.GetFileName(path);
            _imd.Body.ModelInfo.Magnify = _settings.Magnify;
            _imd.Body.ModelInfo.UsePrimitiveStrip = _settings.UsePrimitiveStrip ? "on" : "off";

            GetTextures();
            GetMaterials();
            GetMatrices();
            GetNodes(_scene.RootNode);
            GetPosScales();
            GetPolygons();
        }

        public void Write(string path)
        {
            File.WriteAllBytes(path, _imd.Write());
        }
    }

    public struct ModelBounds
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 BoxXyz => Min;
        public Vector3 BoxWhd => Max - Min;

        public int GetPosScale()
        {
            var maxMax = Math.Abs(Math.Max(Max.X, Math.Max(Max.Y, Max.Z)));
            var minMin = Math.Abs(Math.Min(Min.X, Math.Min(Min.Y, Min.Z)));

            return GetPosScale(Math.Max(maxMax, minMin));
        }

        public int GetBoxPosScale()
        {
            var maxWhd = Math.Abs(Math.Max(BoxWhd.X, Math.Max(BoxWhd.Y, BoxWhd.Z)));
            var minXyz = Math.Abs(Math.Min(BoxXyz.X, Math.Min(BoxXyz.Y, BoxXyz.Z)));

            return GetPosScale(Math.Max(maxWhd, minXyz));
        }

        private static int GetPosScale(float maxCoordinate)
        {
            var maxCoord = (int)Math.Round(maxCoordinate * 4096);

            var posScale = 0;

            while (maxCoord >= 0x8000)
            {
                posScale++;
                maxCoord >>= 1;
            }

            return posScale;
        }

        public static ModelBounds Calculate(Dictionary<int, List<Vector3>> vertices)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);

            foreach (var mesh in vertices.Values)
            {
                foreach (var vtx in mesh)
                {
                    //Set Min
                    if (vtx.X < min.X) min.X = vtx.X;
                    if (vtx.Y < min.Y) min.Y = vtx.Y;
                    if (vtx.Z < min.Z) min.Z = vtx.Z;

                    //Set Max
                    if (vtx.X > max.X) max.X = vtx.X;
                    if (vtx.Y > max.Y) max.Y = vtx.Y;
                    if (vtx.Z > max.Z) max.Z = vtx.Z;
                }
            }

            return new ModelBounds
            {
                Min = min,
                Max = max
            };
        }
    }
}
