using LibNitro.G3D.BinRes;
using LibNitro.Intermediate.Imd;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd
{
    public static class MdlUtils
    {
        public static MDL0.Model GetModel(Imd imd)
        {
            var model = new MDL0.Model
            {
                evpMatrices = null, //todo: Use real value

                info = GetModelInfo(imd)
            };

            return model;
        }


        private static MDL0.Model.ModelInfo GetModelInfo(Imd imd)
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

        
    }
}
