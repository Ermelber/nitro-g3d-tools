using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
using LibFoundation.Math;
using LibNitro.Intermediate;
using LibNitro.Intermediate.Imd;
using LibNitroG3DTools.Stripping;
using LibNitroG3DTools.Utils;
using Material = LibNitro.Intermediate.Imd.Material;
using Node = LibNitro.Intermediate.Imd.Node;
using Primitive = LibNitro.Intermediate.Imd.Primitive;

namespace LibNitroG3DTools.Converter.Intermediate.Imd
{
    public class ImdConverter
    {
        public static readonly GeneratorInfo GeneratorInfo = new GeneratorInfo
        {
            Name    = "ASS to IMD (Made by Ermelber and Gericom)",
            Version = "0.3.1"
        };

        private readonly ImdConverterSettings _settings;
        private readonly Scene                _scene;

        //Used to calculate box pos scale when compress node != unite_combine
        //(You still have to know the baked vertices positions for box_test)
        private readonly Scene _sceneUnite;

        ///Key: Mesh Id, Value: List of vertices in VecFx32
        private Dictionary<int, List<VecFx32>> _meshes = new Dictionary<int, List<VecFx32>>();
        //Key: Mesh Id, Value: Node Index
        private Dictionary<int, sbyte> _meshesNode = new Dictionary<int, sbyte>();
        private List<string> _boneNames = new List<string>();

        private readonly string _modelDirectory;

        private LibNitro.Intermediate.Imd.Imd _imd;

        private void GetPosScales()
        {
            var bounds = ModelBounds.Calculate(_meshes);
            _imd.Body.ModelInfo.PosScale = bounds.GetPosScale();

            if (_settings.CompressNodeMode != "unite_combine")
            {
                bounds = ModelBounds.Calculate(_sceneUnite);
            }

            _imd.Body.BoxTest.PosScale = bounds.GetBoxPosScale();
            _imd.Body.BoxTest.Position = (bounds.BoxXyz >> _imd.Body.BoxTest.PosScale).ToVector3();
            _imd.Body.BoxTest.Size = (bounds.BoxWhd >> _imd.Body.BoxTest.PosScale).ToVector3();
        }

        private void GetTextures()
        {
            var palettes = new List<TexPalette>();
            var textures = new List<TexImage>();

            int texId     = 0;
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
                            Index  = texId++,
                            Name   = texture.TextureName,
                            Path   = texPath,
                            Height = texture.Height,
                            Width  = texture.Width,
                            Bitmap = new TexBitmap
                            {
                                Size  = texture.BitmapSize,
                                Value = texture.BitmapData
                            },
                            Format         = texture.Format,
                            OriginalHeight = texture.Height,
                            OriginalWidth  = texture.Width,
                            PaletteName    = texture.PaletteName
                        };

                        if (texture.Format == "tex4x4")
                        {
                            texImage.Tex4x4PaletteIndex = new Tex4x4PaletteIndex
                            {
                                Size  = texture.Tex4x4PaletteIndexSize,
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
                                Index     = paletteId++,
                                Name      = texture.PaletteName,
                                Value     = texture.PaletteData
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

        private Dictionary<int, int> GetMaterials()
        {
            var matMap = new Dictionary<int, int>();
            int i = 0;
            int j = 0;
            foreach (var mat in _scene.Materials)
            {
                if (_scene.Meshes.All(a => a.MaterialIndex != j))
                {
                    j++;
                    continue;
                }

                matMap.Add(j, i);
                var material = new Material
                {
                    Index = i,
                    Name = mat.Name.Length > 16
                        ? $"{mat.Name.Substring(0, 13)}{i}"
                        : mat.Name, //Fixes the length of the material name
                    Alpha = (byte) (mat.Opacity * 31),
                    Ambient =
                        $"{(int) (mat.ColorAmbient.R * 31)} {(int) (mat.ColorAmbient.G * 31)} {(int) (mat.ColorAmbient.B * 31)}",
                    Diffuse =
                        $"{(int) (mat.ColorDiffuse.R * 31)} {(int) (mat.ColorDiffuse.G * 31)} {(int) (mat.ColorDiffuse.B * 31)}",
                    Emission =
                        $"{(int) (mat.ColorEmissive.R * 31)} {(int) (mat.ColorEmissive.G * 31)} {(int) (mat.ColorEmissive.B * 31)}",
                    Specular =
                        $"{(int) (mat.ColorSpecular.R * 31)} {(int) (mat.ColorSpecular.G * 31)} {(int) (mat.ColorSpecular.B * 31)}",
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

                    material.TexScale     = "1.0000 1.0000";
                    material.TexRotate    = "0.0000";
                    material.TexTranslate = "0.0000 0.0000";

                    material.TexGenMode = "none";
                }

                _imd.Body.MaterialArray.Materials.Add(material);

                i++;
                j++;
            }

            _imd.Body.ModelInfo.MaterialSize = $"{matMap.Count} {matMap.Count}";
            return matMap;
        }

        private void GetMatrices()
        {
            var mtxCount = 0;

            foreach (var node in _imd.Body.NodeArray.Nodes)
            {
                if (node.Kind != "mesh" && node.Kind != "joint") continue;

                _imd.Body.MatrixArray.Matrices.Add(new Matrix
                {
                    Index = mtxCount++,
                    MatrixWeight = 1,
                    NodeIndex = (byte)node.Index
                });
            }
        }

        private void GetPolygons(Dictionary<int, int> matMap)
        {
            var posScale = _imd.Body.ModelInfo.PosScale;

            var polygonId = 0;
            foreach (var mesh in _meshes)
            {
                var meshId     = mesh.Key;
                var sceneMesh  = _scene.Meshes[meshId];
                var materialId = matMap[sceneMesh.MaterialIndex];
                var material   = _imd.Body.MaterialArray.Materials[materialId];
                var useNormals = material.Light0 == "on" || material.Light1 == "on" || material.Light2 == "on" ||
                                 material.Light3 == "on";

                var texture = material.TexImageIdx != -1
                    ? _imd.Body.TexImageArray.TexImages[material.TexImageIdx]
                    : null;

                var polygon = new Polygon
                {
                    Index = polygonId,
                    Name  = $"polygon{polygonId}",
                    ClrFlag = !useNormals && sceneMesh.HasVertexColors(0) ? "on" : "off",
                    TexFlag = texture != null ? "on" : "off",
                    NrmFlag = useNormals ? "on" : "off",
                };

                var matrixPrimitive = new MatrixPrimitive
                {
                    Index = 0,
                    PrimitiveArray = new PrimitiveArray
                    {
                        Primitives = new List<Primitive>()
                    }
                };
                
                //Get matrix list from node index
                foreach (var mtx in _imd.Body.MatrixArray.Matrices.Where(x => x.NodeIndex == _meshesNode[meshId]))
                {
                    matrixPrimitive.MtxList.List.Add(mtx.Index);
                }

                //Get joints matrices
                if (sceneMesh.HasBones)
                {
                    foreach (var boneName in sceneMesh.Bones.Select(x => x.Name))
                    {
                        foreach (var jointNode in _imd.Body.NodeArray.Nodes.Where(x => x.Name == boneName && x.Kind == "joint"))
                        {
                            matrixPrimitive.MtxList.List.Add(_imd.Body.MatrixArray.Matrices.First(x => x.NodeIndex == jointNode.Index).Index);
                        }
                    }
                }
                
                polygon.MatrixPrimitives.Add(matrixPrimitive);

                var finalPrimList = new List<Primitive>();

                var primList = new List<Stripping.Primitive>();
                var posList  = new Dictionary<VecFx32, int>();
                var clrList  = new Dictionary<(int r, int g, int b), int>();
                var texList  = new Dictionary<(int s, int t), int>();
                var nrmList  = new Dictionary<(int x, int y, int z), int>();

                foreach (var face in sceneMesh.Faces)
                {
                    var prim = new Stripping.Primitive();
                    if (face.IndexCount == 3)
                    {
                        prim.Type = Stripping.Primitive.PrimitiveType.Triangles;
                        polygon.TriangleSize++;
                    }
                    else if (face.IndexCount == 4)
                    {
                        prim.Type = Stripping.Primitive.PrimitiveType.Quads;
                        polygon.QuadSize++;
                    }
                    else
                        continue; //Other shapes????!

                    polygon.PolygonSize++;

                    foreach (var vertexIndex in face.Indices)
                    {
                        var vertex = _meshes[meshId][vertexIndex];

                        prim.Matrices.Add(-1);

                        //Vertex Colors
                        if (!useNormals && sceneMesh.HasVertexColors(0))
                        {
                            var c = sceneMesh.VertexColorChannels[0][vertexIndex];
                            var clr = ((int) Math.Round(c.R * 31), (int) Math.Round(c.G * 31), (int) Math.Round(c.B * 31));

                            int clrIdx = clrList.Count;
                            if (clrList.ContainsKey(clr))
                                clrIdx = clrList[clr];
                            else
                                clrList.Add(clr, clrIdx);
                            prim.Colors.Add(clrIdx);
                        }
                        else
                            prim.Colors.Add(-1);

                        //Texture Coordinates
                        if (texture != null)
                        {
                            var texCoord = sceneMesh.TextureCoordinateChannels[0][vertexIndex];
                            var texC = ((int) Math.Round(texCoord.X * texture.Width * 16),
                                (int) Math.Round((-texCoord.Y * texture.Height + texture.Height) * 16));

                            int texIdx = texList.Count;
                            if (texList.ContainsKey(texC))
                                texIdx = texList[texC];
                            else
                                texList.Add(texC, texIdx);
                            prim.TexCoords.Add(texIdx);
                        }
                        else
                            prim.TexCoords.Add(-1);

                        //Normals
                        if (useNormals)
                        {
                            var normal = sceneMesh.Normals[vertexIndex];
                            int nx     = (int) Math.Round(normal.X * 512) & 0x3FF;
                            if (nx == 512)
                                nx--;
                            int ny = (int) Math.Round(normal.Y * 512) & 0x3FF;
                            if (ny == 512)
                                ny--;
                            int nz = (int) Math.Round(normal.Z * 512) & 0x3FF;
                            if (nz == 512)
                                nz--;
                            var nrm = (nx, ny, nz);

                            int nrmIdx = nrmList.Count;
                            if (nrmList.ContainsKey(nrm))
                                nrmIdx = nrmList[nrm];
                            else
                                nrmList.Add(nrm, nrmIdx);
                            prim.Normals.Add(nrmIdx);
                        }
                        else
                            prim.Normals.Add(-1);

                        //Vertex
                        var posScaled = vertex >> posScale;

                        int posIdx = posList.Count;
                        if (posList.ContainsKey(posScaled))
                            posIdx = posList[posScaled];
                        else
                            posList.Add(posScaled, posIdx);
                        prim.Positions.Add(posIdx);
                    }

                    prim.VertexCount = face.IndexCount;
                    primList.Add(prim);
                }

                var poss = posList.Keys.ToArray();
                var clrs = clrList.Keys.ToArray();
                var texs = texList.Keys.ToArray();
                var nrms = nrmList.Keys.ToArray();

                Stripping.Primitive[] newPrims;

                if (_settings.UsePrimitiveStrip)
                {
                    newPrims = QuadStripper.Process(primList.ToArray());
                    newPrims = TriStripper.Process(newPrims);
                }
                else
                {
                    //No stripping
                    newPrims = primList.ToArray();
                }

                //merge all tris and quads
                {
                    var tmp = new List<Stripping.Primitive>(newPrims.Where(a =>
                        a.Type == Stripping.Primitive.PrimitiveType.TriangleStrip ||
                        a.Type == Stripping.Primitive.PrimitiveType.QuadStrip));
                    var tris  = new Stripping.Primitive {Type = Stripping.Primitive.PrimitiveType.Triangles};
                    var quads = new Stripping.Primitive {Type = Stripping.Primitive.PrimitiveType.Quads};
                    foreach (var p in newPrims)
                    {
                        if (p.Type == Stripping.Primitive.PrimitiveType.Triangles)
                        {
                            tris.AddVtx(p, 0);
                            tris.AddVtx(p, 1);
                            tris.AddVtx(p, 2);
                        }
                        else if(p.Type == Stripping.Primitive.PrimitiveType.Quads)
                        {
                            quads.AddVtx(p, 0);
                            quads.AddVtx(p, 1);
                            quads.AddVtx(p, 2);
                            quads.AddVtx(p, 3);
                        }
                    }
                    if(tris.VertexCount != 0)
                        tmp.Add(tris);
                    if(quads.VertexCount != 0)
                        tmp.Add(quads);
                    newPrims = tmp.ToArray();
                }

                Array.Sort(newPrims);

                foreach (var prim in newPrims)
                {
                    var primitive = new Primitive();
                    switch (prim.Type)
                    {
                        case Stripping.Primitive.PrimitiveType.Triangles:
                            primitive.Type = "triangles";
                            break;
                        case Stripping.Primitive.PrimitiveType.Quads:
                            primitive.Type = "quads";
                            break;
                        case Stripping.Primitive.PrimitiveType.TriangleStrip:
                            primitive.Type = "triangle_strip";
                            break;
                        case Stripping.Primitive.PrimitiveType.QuadStrip:
                            primitive.Type = "quad_strip";
                            break;
                        default:
                            throw new Exception("Unexpected primitive type!");
                    }

                    var prevVtx = new VecFx32();
                    int prevClrIdx = -1;

                    for (int i = 0; i < prim.VertexCount; i++)
                    {
                        if (texture != null)
                            primitive.Commands.Add(new TextureCoordCommand(texs[prim.TexCoords[i]].s / 16f,
                                texs[prim.TexCoords[i]].t / 16f));

                        if (useNormals)
                            primitive.Commands.Add(new NormalCommand((nrms[prim.Normals[i]].x / 512f,
                                nrms[prim.Normals[i]].y / 512f, nrms[prim.Normals[i]].z / 512f)));
                        else if (!useNormals && sceneMesh.HasVertexColors(0) && prevClrIdx != prim.Colors[i])
                        {
                            primitive.Commands.Add(new ColorCommand(clrs[prim.Colors[i]].r, clrs[prim.Colors[i]].g,
                                clrs[prim.Colors[i]].b));
                            prevClrIdx = prim.Colors[i];
                        }

                        var vtx = poss[prim.Positions[i]];
                        var diff = vtx - prevVtx;

                        if (i != 0 && diff.X == 0)
                            primitive.Commands.Add(new PosYzCommand(vtx.ToVector3()));
                        else if (i != 0 && diff.Y == 0)
                            primitive.Commands.Add(new PosXzCommand(vtx.ToVector3()));
                        else if (i != 0 && diff.Z == 0)
                            primitive.Commands.Add(new PosXyCommand(vtx.ToVector3()));
                        else if (i != 0 &&
                                 diff.X < 512 && diff.X >= -512 &&
                                 diff.Y < 512 && diff.Y >= -512 &&
                                 diff.Z < 512 && diff.Z >= -512)
                            primitive.Commands.Add(new PosDiffCommand(diff.ToVector3()));
                        else if ((vtx.X & 0x3F) == 0 && (vtx.Y & 0x3F) == 0 && (vtx.Z & 0x3F) == 0)
                            primitive.Commands.Add(new PosShortCommand(vtx.ToVector3()));
                        else
                            primitive.Commands.Add(new PosXyzCommand(vtx.ToVector3()));

                        prevVtx = vtx;

                        polygon.VertexSize++;
                        primitive.VertexSize++;
                    }

                    primitive.Index = finalPrimList.Count;
                    finalPrimList.Add(primitive);
                }

                if (finalPrimList.Count == 0)
                    continue;

                finalPrimList[0].Commands.Insert(0, new MatrixCommand {Index = 0});

                matrixPrimitive.PrimitiveArray.Primitives.AddRange(finalPrimList);

                _imd.Body.PolygonArray.Polygons.Add(polygon);
                _imd.Body.OutputInfo.PolygonSize  += polygon.PolygonSize;
                _imd.Body.OutputInfo.TriangleSize += polygon.TriangleSize;
                _imd.Body.OutputInfo.QuadSize     += polygon.QuadSize;
                _imd.Body.OutputInfo.VertexSize   += polygon.VertexSize;

                var node = _imd.Body.NodeArray.Nodes[_meshesNode[meshId]];

                node.PolygonSize += polygon.PolygonSize;
                node.TriangleSize += polygon.TriangleSize;
                node.QuadSize += polygon.QuadSize;
                node.VertexSize += polygon.VertexSize;

                node.Displays.Add(new NodeDisplay
                {
                    Index = node.Displays.Count,
                    Polygon = polygonId,
                    Material = materialId,
                    Priority = 0
                });

                polygonId++;
            }
        }

        private void GetBoneNames()
        {
            foreach (var mesh in _scene.Meshes)
            {
                if (!mesh.HasBones) continue;

                foreach (var bone in mesh.Bones)
                {
                    if (!_boneNames.Contains(bone.Name))
                        _boneNames.Add(bone.Name);
                }
            }
        }

        /// <summary>
        /// Collects a single mesh vertices
        /// </summary>
        /// <param name="meshId"></param>
        private void CollectMesh(int meshId)
        {
            var mesh = _scene.Meshes[meshId];

            _meshes.Add(meshId, new List<VecFx32>());

            foreach (var originalVtx in mesh.Vertices)
            {
                var vtx = _settings.FlipYZ ? 
                    new VecFx32(originalVtx.X, originalVtx.Z, originalVtx.Y) : 
                    new VecFx32(originalVtx.X, originalVtx.Y, originalVtx.Z);

                _meshes[meshId].Add(vtx);
            }
        }

        private void CollectNodeMeshes(Assimp.Node node, sbyte nodeIndex = 0)
        {
            foreach (var meshId in node.MeshIndices)
            {
                _meshesNode.Add(meshId, nodeIndex);
                CollectMesh(meshId);
            }
        }

        private static sbyte _nodeCount = 0;

        private string DetermineKind(Assimp.Node node)
        {
            if (node.HasMeshes)
                return "mesh";

            return _boneNames.Contains(node.Name) ? "joint" : "null";
        }
        
        //compress_mode = none
        private void GetUncompressNodes(Assimp.Node node, sbyte parent = -1, sbyte broPrev = -1, sbyte broNext = -1)
        {
            var kind = DetermineKind(node);
            
            var index = _nodeCount++;

            node.Transform.Decompose(out var sV, out var rQ, out var tV);
            var transVec = new VecFx32(tV.X, tV.Y, tV.Z).ToVector3();
            var scaleVec = new VecFx32(sV.X, sV.Y, sV.Z).ToVector3();
            //turn quaternion into angle vector
            var rotVec = new VecFx32(rQ.ToEulerAngles()).ToVector3();
            
            var nodeItem = new Node
            {
                Index = index,
                Name = node.Name,
                Kind = kind,
                Parent = parent,
                BrotherPrev = broPrev,
                BrotherNext = broNext,
                Child = node.HasChildren ? (sbyte)(index + 1) : (sbyte)-1,
                Translate = $"{transVec.X} {transVec.Y} {transVec.Z}",
                Scale = $"{scaleVec.X} {scaleVec.Y} {scaleVec.Z}",
                Rotate = $"{rotVec.X} {rotVec.Y} {rotVec.Z}"
            };

            _imd.Body.NodeArray.Nodes.Add(nodeItem);

            if (node.HasMeshes)
            {
                CollectNodeMeshes(node, index);
            }

            sbyte childIndex = index;

            foreach (var child in node.Children)
            {
                GetUncompressNodes(
                    child, 
                    index, 
                    childIndex > 0 ? (sbyte)(childIndex) : (sbyte)-1,
                    childIndex < node.ChildCount - 1 ? (sbyte)(childIndex + 2) : (sbyte)-1);
                childIndex++;
            }
        }

        //compress_mode = unite_combine
        private void GetUniteCombineNodes(Assimp.Node node)
        {
            //The first time adds the root node
            if (_imd.Body.NodeArray.Nodes.Count == 0)
            {
                _imd.Body.ModelInfo.NodeSize = "1 1";

                _imd.Body.NodeArray.Nodes.Add(new Node
                {
                    Index = 0,
                    Name = "world_root",
                    Kind = "mesh",
                    Parent = -1
                });
            }

            if (node.HasMeshes)
            {
                CollectNodeMeshes(node);
            }

            foreach (var children in node.Children)
            {
                GetUniteCombineNodes(children);
            }
        }

        private void GetNodes(Assimp.Node node)
        {
            switch (_settings.CompressNodeMode)
            {
                case "none":
                case "cull":
                case "merge":
                    GetBoneNames();
                    GetUncompressNodes(node);
                    break;
                case "unite_combine":
                    GetUniteCombineNodes(node);
                    break;
                default:
                    throw new NotSupportedException($"Compress Node Mode \"{_settings.CompressNodeMode}\" is not supported.");
            }
        }

        public ImdConverter(string path, ImdConverterSettings settings = null)
        {
            _settings = settings ?? new ImdConverterSettings();

            var context = new AssimpContext();

            //Magnify model
            context.Scale = _settings.Magnify;
            
            _sceneUnite = context.ImportFile(path, PostProcessSteps.PreTransformVertices);
            _scene = _settings.CompressNodeMode == "unite_combine" ? _sceneUnite : context.ImportFile(path);

            _imd = new LibNitro.Intermediate.Imd.Imd
            {
                Head = {GeneratorInfo         = GeneratorInfo},
                Body = {OriginalGeneratorInfo = GeneratorInfo}
            };

            _modelDirectory                       = Path.GetDirectoryName(path);
            _imd.Head.CreateInfo.Source           = Path.GetFileName(path);
            _imd.Body.ModelInfo.Magnify           = _settings.Magnify;
            _imd.Body.ModelInfo.UsePrimitiveStrip = _settings.UsePrimitiveStrip ? "on" : "off";
            _imd.Body.ModelInfo.CompressNode = _settings.CompressNodeMode;

            GetTextures();
            var matMap = GetMaterials();
            GetNodes(_scene.RootNode);
            GetMatrices();
            GetPosScales();
            GetPolygons(matMap);
        }

        public void Write(string path)
        {
            File.WriteAllBytes(path, _imd.Write());
        }
    }

    public struct ModelBounds
    {
        public VecFx32 Min;
        public VecFx32 Max;
        public VecFx32 BoxXyz => Min;
        public VecFx32 BoxWhd => Max - Min;

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

        private static int GetPosScale(int maxCoord)
        {
            var posScale = 0;

            while (maxCoord >= 0x8000)
            {
                posScale++;
                maxCoord >>= 1;
            }

            return posScale;
        }

        public static ModelBounds Calculate(Dictionary<int, List<VecFx32>> vertices)
        {
            var min = new VecFx32(int.MaxValue);
            var max = new VecFx32(int.MinValue);

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

        public static ModelBounds Calculate(Scene scene)
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            foreach (var mesh in scene.Meshes)
            {
                foreach (var vtx in mesh.Vertices)
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
                Min = new VecFx32(min),
                Max = new VecFx32(max)
            };
        }
    }
}