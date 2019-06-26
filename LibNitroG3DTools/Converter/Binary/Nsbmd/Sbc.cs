using System;
using System.Collections.Generic;
using LibEndianBinaryIO;
using static LibNitro.G3D.Sbc;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd
{
    public class Sbc
    {
        public class Command
        {
            public Commands Type;
            public List<byte> Params;

            public void Write(EndianBinaryWriterEx ew)
            {
                ew.Write((byte) Type);
                ew.Write(Params.ToArray(), 0, Params.Count);
            }

            public override string ToString() => $"{Type}";
        }

        public Sbc()
        {

        }

        public Sbc(byte[] sbc)
        {
            for (int i = 0; i < sbc.Length; i++)
            {
                var cmd = (Commands) sbc[i];

                switch (cmd)
                {
                    case Commands.Nop:
                        NOP();
                        break;
                    case Commands.Ret:
                        RET();
                        break;
                    case Commands.Node:
                        //NODE();
                        break;
                    case Commands.Mtx:
                        break;
                    case Commands.Mat:
                        break;
                    case Commands.Nodedesc:
                        break;
                    case Commands.BB:
                        break;
                    case Commands.BBY:
                        break;
                    case Commands.Nodemix:
                        break;
                    case Commands.CallDl:
                        break;
                    case Commands.Posscale:
                        break;
                    case Commands.Envmap:
                        break;
                    case Commands.Prjmap:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        List<Command> CommandList = new List<Command>();
        List<byte> Data = new List<byte>();
        /// <summary>
        /// No operation.
        /// </summary>
        public void NOP()
        {
            Data.Add((byte) Commands.Nop);
        }
        /// <summary>
        /// Exists at the end of the SBC line.
        /// </summary>
        public void RET()
        {
            Data.Add((byte) Commands.Ret);
        }
        /// <summary>
        /// All MAT and SHP commands before the next NODE command appears are regarded as belonging to the node that has the NodeID specified by the NODE command.
        /// </summary>
        /// <param name="NodeID">Specifies node corresponding to node ID.</param>
        /// <param name="V">True when shape belonging to NodeID is visible. False when it is invisible.</param>
        public void NODE(byte NodeID, bool V)
        {
            Data.Add((byte) Commands.Node);
            Data.Add(NodeID);
            Data.Add((byte)(V ? 1 : 0));
        }
        /// <summary>
        /// Issues RestoreMtx command. Reads matrix from specified location of matrix stack of location coordinate matrix to current matrix.
        /// </summary>
        /// <param name="Idx">Matrix stack index.</param>
        public void MTX(byte Idx)
        {
            Data.Add(3);
            Data.Add(Idx);
        }
        /// <summary>
        /// Sets the settings of the specified material to geometry engine.
        /// </summary>
        /// <param name="MatID">Material ID</param>
        public void MAT(byte MatID)
        {
            Data.Add(4 | (1 << 5));
            Data.Add(MatID);
        }
        /// <summary>
        /// Draws specified shape.
        /// </summary>
        /// <param name="ShpID">Shape ID</param>
        public void SHP(byte ShpID)
        {
            Data.Add(5);
            Data.Add(ShpID);
        }
        /// <summary>
        /// Calculates modeling matrix corresponding to node ID.
        /// </summary>
        /// <param name="NodeID">Specifies node ID that requires modeling matrix.</param>
        /// <param name="ParentNodeID">Specifies ID of parent node.</param>
        /// <param name="S">Maya’s Segment Scale Compensate is applied to this node.</param>
        /// <param name="P">This node is the parent node of the node with Maya’s Segment ScaleCompensate applied.</param>
        /// <param name="SrcIdx">Matrix stack index is specified when restoring matrix from matrix stack before calculation. Specified when extracting matrix corresponding to parent node from matrix stack.</param>
        public void NODEDESC(byte NodeID, byte ParentNodeID, bool S, bool P, byte SrcIdx)
        {
            NODEDESC(NodeID, ParentNodeID, S, P, -1, SrcIdx);
        }
        /// <summary>
        /// Calculates modeling matrix corresponding to node ID.
        /// </summary>
        /// <param name="NodeID">Specifies node ID that requires modeling matrix.</param>
        /// <param name="ParentNodeID">Specifies ID of parent node.</param>
        /// <param name="S">Maya’s Segment Scale Compensate is applied to this node.</param>
        /// <param name="P">This node is the parent node of the node with Maya’s Segment ScaleCompensate applied.</param>
        /// <param name="DestIdx">Matrix stack index is specified when storing calculation results in matrix stack. Specified when it is necessary to store calculation results in matrix stack.</param>
        /// <param name="SrcIdx">Matrix stack index is specified when restoring matrix from matrix stack before calculation. Specified when extracting matrix corresponding to parent node from matrix stack.</param>
        public void NODEDESC(byte NodeID, byte ParentNodeID, bool S, bool P, int DestIdx = -1, int SrcIdx = -1)
        {
            Data.Add((byte)(6 | (((SrcIdx != -1) ? 1 : 0) << 6) | (((DestIdx != -1) ? 1 : 0) << 5)));
            Data.Add(NodeID);
            Data.Add(ParentNodeID);
            Data.Add((byte)(((P ? 1 : 0) << 1) | (S ? 1 : 0)));
            if (DestIdx != -1) Data.Add((byte)DestIdx);
            if (SrcIdx != -1) Data.Add((byte)SrcIdx);
        }
        /// <summary>
        /// Applies billboard conversion to matrix.
        /// </summary>
        /// <param name="NodeID">Node ID of matrix that applies billboard conversion.</param>
        /// <param name="SrcIdx">Matrix stack index is specified when restoring matrix from matrix stack before calculation.</param>
        public void BB(byte NodeID, byte SrcIdx)
        {
            BB(NodeID, -1, SrcIdx);
        }
        /// <summary>
        /// Applies billboard conversion to matrix.
        /// </summary>
        /// <param name="NodeID">Node ID of matrix that applies billboard conversion.</param>
        /// <param name="DestIdx">Matrix stack index is specified when storing calculation results in matrix stack.</param>
        /// <param name="SrcIdx">Matrix stack index is specified when restoring matrix from matrix stack before calculation.</param>
        public void BB(byte NodeID, int DestIdx = -1, int SrcIdx = -1)
        {
            Data.Add((byte)(7 | (((SrcIdx != -1) ? 1 : 0) << 6) | (((DestIdx != -1) ? 1 : 0) << 5)));
            Data.Add(NodeID);
            if (DestIdx != -1) Data.Add((byte)DestIdx);
            if (SrcIdx != -1) Data.Add((byte)SrcIdx);
        }
        /// <summary>
        /// Applies Y axis billboard conversion to matrix.
        /// </summary>
        /// <param name="NodeID">Node ID of matrix that applies Y axis billboard conversion.</param>
        /// <param name="SrcIdx">Matrix stack is specified when restoring matrix from matrix stack before calculation.</param>
        public void BBY(byte NodeID, byte SrcIdx)
        {
            BB(NodeID, -1, SrcIdx);
        }
        /// <summary>
        /// Applies Y axis billboard conversion to matrix.
        /// </summary>
        /// <param name="NodeID">Node ID of matrix that applies Y axis billboard conversion.</param>
        /// <param name="DestIdx">Matrix stack index is specified when storing calculation results in matrix stack.</param>
        /// <param name="SrcIdx">Matrix stack is specified when restoring matrix from matrix stack before calculation.</param>
        public void BBY(byte NodeID, int DestIdx = -1, int SrcIdx = -1)
        {
            Data.Add((byte)(8 | (((SrcIdx != -1) ? 1 : 0) << 6) | (((DestIdx != -1) ? 1 : 0) << 5)));
            Data.Add(NodeID);
            if (DestIdx != -1) Data.Add((byte)DestIdx);
            if (SrcIdx != -1) Data.Add((byte)SrcIdx);
        }
        /// <summary>
        /// Blends location coordinate matrix with specified ratio, and calculates matrix for weighted envelope. When calculating, uses inverse matrix (to convert from coordinate system of entire model to coordinate system of each joint) of modeling conversion matrix stored in evpMatrices.
        /// </summary>
        /// <param name="DestIdx">Index of matrix stack where calculation results matrix is stored.</param>
        /// <param name="NumMtx">Number of blended matrices.</param>
        /// <param name="SrcIdx">Index of matrix stack where blended matrix is stored.</param>
        /// <param name="NodeID">Node ID of blended matrix.</param>
        /// <param name="Ratio">Matrix blend ratio is a fixed decimal with unsigned decimal of 8 bits.</param>
        public void NODEMIX(byte DestIdx, byte NumMtx, byte[] SrcIdx, byte[] NodeID, byte[] Ratio)
        {
            Data.Add(9);
            Data.Add(DestIdx);
            Data.Add(NumMtx);
            for (int i = 0; i < NumMtx; i++)
            {
                Data.Add(SrcIdx[i]);
                Data.Add(NodeID[i]);
                Data.Add(Ratio[i]);
            }
        }
        /// <summary>
        /// Sends display list specified with operand to geometry engine.
        /// </summary>
        /// <param name="RelAddr">Relative address from start address of CALLDL instruction to display list.</param>
        /// <param name="Size">Size of display list (in bytes).</param>
        public void CALLDL(UInt32 RelAddr, UInt32 Size)
        {
            Data.Add(10);
            Data.AddRange(BitConverter.GetBytes(RelAddr));
            Data.AddRange(BitConverter.GetBytes(Size));
        }
        /// <summary>
        /// When OPT=true, applies scaling matrix (see posScale and invPosScale ofModelInfo) set for each model data in current matrix. When OPT=false, applies the inverse matrix.
        /// </summary>
        public void POSSCALE(bool OPT)
        {
            Data.Add((byte)(11 | ((OPT ? 0 : 1) << 5)));
        }
        /// <summary>
        /// Calculates the texture matrix for environmental mapping. It is placed immediately after the MAT command.
        /// </summary>
        /// <param name="MatID">Material ID</param>
        /// <param name="Flag">Flag for expansion (Currently always 0)</param>
        public void ENVMAP(byte MatID, byte Flag)
        {
            Data.Add(12);
            Data.Add(MatID);
            Data.Add(Flag);
        }
        /// <summary>
        /// Calculates the texture matrix for projection mapping. It is placed immediately after the MAT command.
        /// </summary>
        /// <param name="MatID">Material ID</param>
        /// <param name="Flag">Flag for expansion (Currently always 0)</param>
        public void PRJMAP(byte MatID, byte Flag)
        {
            Data.Add(13);
            Data.Add(MatID);
            Data.Add(Flag);
        }

        public byte[] GetData()
        {
            return Data.ToArray();
        }
    }
}