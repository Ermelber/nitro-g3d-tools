using System;
using LibNitro.G3D.BinRes;
using LibNitro.Intermediate.Imd;
using static LibNitro.G3D.BinRes.MDL0;
using static LibNitro.G3D.BinRes.MDL0.Model;
using static LibNitro.G3D.BinRes.MDL0.Model.MaterialSet;
using static LibNitro.G3D.BinRes.MDL0.Model.MaterialSet.Material.NNS_G3D_MATFLAG;
using static LibNitro.G3D.BinRes.MDL0.Model.NodeSet;
using static LibNitro.G3D.BinRes.MDL0.Model.ShapeSet;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd
{
    public static class MdlUtils
    {
        public static Model GetModel(Imd imd)
        {
            var model = new Model
            {
                evpMatrices = null, //todo: Use real value
                info = GetModelInfo(imd),
                nodes = GetNodeSet(imd),
                materials = GetMaterialSet(imd),
                shapes = GetShapeSet(imd),
                //Todo: SBC
            };

            return model;
        }

        private static Model.ModelInfo GetModelInfo(Imd imd)
        {
            return new Model.ModelInfo
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

        private static NodeSet GetNodeSet(Imd imd)
        {
            var nodes = new NodeSet
            {
                dict = new Dictionary<NodeSetData>()
            };

            foreach (var node in imd.Body.NodeArray.Nodes)
            {
                nodes.dict.Add(node.Name, GetNodeSetData(node) );
            }

            nodes.data = GetNodeData(imd);

            return nodes;
        }

        private static NodeSetData GetNodeSetData(Node imdNode)
        {
            return new NodeSetData();
        }

        private static NodeData[] GetNodeData(Imd imd)
        {
            return new[]
            {
                new NodeData
                {
                    //TODO: Matrix
                    flag = 0xf807 //?
                }
            };
        }

        public static MaterialSet GetMaterialSet(Imd imd)
        {
            var matSet = new MaterialSet
            {
                dict = new Dictionary<MaterialSetData>(),
                dictPlttToMatList = new Dictionary<PlttToMatData>(),
                dictTexToMatList = new Dictionary<TexToMatData>(),
                materials = new MaterialSet.Material[imd.Body.MaterialArray.Materials.Count]
            };

            byte matIdx = 0;

            foreach (var mat in imd.Body.MaterialArray.Materials)
            {
                ushort texH = 0, texW = 0;

                if (mat.TexImageIdx != -1)
                {
                    var texName = imd.Body.TexImageArray.TexImages[mat.TexImageIdx].Name;
                    var palName = imd.Body.TexImageArray.TexImages[mat.TexImageIdx].PaletteName;

                    texH = (ushort) imd.Body.TexImageArray.TexImages[mat.TexImageIdx].OriginalHeight;
                    texW = (ushort)imd.Body.TexImageArray.TexImages[mat.TexImageIdx].OriginalWidth;

                    if (!matSet.dictTexToMatList.Contains(texName))
                    {
                        matSet.dictTexToMatList.Add(texName, new TexToMatData
                        {
                            NrMat = 1,
                            Materials = new [] {matIdx}
                        });
                    }
                    else
                    {
                        var entry = matSet.dictTexToMatList[texName];
                        entry.NrMat++;
                        var newMats = new byte[entry.NrMat];
                        entry.Materials.CopyTo(newMats, 0);
                        entry.Materials = newMats;
                    }

                    if (!matSet.dictPlttToMatList.Contains(palName))
                    {
                        matSet.dictPlttToMatList.Add(palName, new PlttToMatData
                        {
                            NrMat = 1,
                            Materials = new[] { matIdx }
                        });
                    }
                    else
                    {
                        var entry = matSet.dictPlttToMatList[palName];
                        entry.NrMat++;
                        var newMats = new byte[entry.NrMat];
                        entry.Materials.CopyTo(newMats, 0);
                        entry.Materials = newMats;
                    }
                }

                matSet.dict.Add(mat.Name, new MaterialSetData());
                matSet.materials[matIdx] = new MaterialSet.Material();
                var curMat = matSet.materials[matIdx];

                curMat.SetAmbient(mat.GetAmbient());
                curMat.SetDiffuse(mat.GetDiffuse());
                curMat.SetEmission(mat.GetEmission());
                curMat.SetSpecular(mat.GetSpecular());

                curMat.SetAlpha(mat.Alpha);

                curMat.flag = TEXMTX_TRANSZERO | TEXMTX_ROTZERO | TEXMTX_SCALEONE | DIFFUSE | VTXCOLOR | SHININESS |
                              AMBIENT | EMISSION | SPECULAR;

                if (mat.TexImageIdx != -1)
                {
                    curMat.origHeight = texH;
                    curMat.origWidth = texW;
                }

                //Todo: use real value
                curMat.magH = 1;
                curMat.magW = 1;

                matIdx++;
            }

            return matSet;
        }

        private static ShapeSet GetShapeSet(Imd imd)
        {
            var shapes = new ShapeSet
            {
                dict = new Dictionary<ShapeSetData>(),
                shape = new Shape[imd.Body.PolygonArray.Polygons.Count]
            };

            int idx = 0;

            foreach (var polygon in imd.Body.PolygonArray.Polygons)
            {
                shapes.dict.Add(polygon.Name, new ShapeSetData());

                //TODO
                shapes.shape[idx++] = new Shape
                {
                    flag = Shape.NNS_G3D_SHPFLAG.NNS_G3D_SHPFLAG_USE_TEXCOORD,
                    DL = G3dDisplayList.Encode(polygon.MatrixPrimitives[0].PrimitiveArray.GetDecodedCommands())
                };
            }

            return shapes;
        }
    }
}
