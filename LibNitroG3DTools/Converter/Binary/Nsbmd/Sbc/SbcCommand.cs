using System.Collections.Generic;
using System.Dynamic;
using LibEndianBinaryIO;
using static LibNitro.G3D.Sbc;

namespace LibNitroG3DTools.Converter.Binary.Nsbmd.Sbc
{
    public abstract class SbcCommand
    {
        public virtual Commands Type { get; set; }
        public byte Option;
        //public byte ByteType;
        protected List<byte> Params = new List<byte>();

        public virtual void Write(EndianBinaryWriterEx ew)
        {
            ew.Write((byte)(((byte)Type & 0x1F) | (Option << 5)));
            ew.Write(Params.ToArray(), 0, Params.Count);
        }

        public override string ToString() => $"{Type}";

        public static SbcCommand Read(EndianBinaryReaderEx er)
        {
            var byteCmd = er.ReadByte();
            var cmd = (Commands)(byteCmd & 0x1F);
            var opt = (byte)((byteCmd & 0xE0) >> 5);
            //var cmdItem = new SbcCommand { Type = cmd, Operator = opt };

            switch (cmd)
            {
                case Commands.Nop:
                    return new SbcNop();
                case Commands.Ret:
                    return new SbcRet();
                case Commands.Node:
                    return new SbcNode(er);
                case Commands.Mtx:
                    return new SbcMtx(er);
                case Commands.Mat:
                    return new SbcMat(opt, er);
                case (Commands)5://Commands.Shp:
                    return new SbcShape(er);
                case Commands.Nodedesc:
                    return new SbcNodeDesc(opt, er);
                case Commands.BB:
                    return new SbcBillboard(opt, er);
                case Commands.BBY:
                    return new SbcBillboardY(opt, er);
                case Commands.Nodemix:
                    return new SbcNodeMix(er);
                case Commands.CallDl:
                    return new SbcCallDl(er);
                case Commands.Posscale:
                    return new SbcPosScale(opt, er);
                case Commands.Envmap:
                    return new SbcEnvMap(er);
                case Commands.Prjmap:
                    return new SbcPrjMap(er);
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// No operation.
    /// </summary>
    public class SbcNop : SbcCommand
    {
        public override Commands Type => Commands.Nop;
    }

    /// <summary>
    /// Exists at the end of the SBC line.
    /// </summary>
    public class SbcRet : SbcCommand
    {
        public override Commands Type => Commands.Ret;
    }

    /// <summary>
    /// All MAT and SHP commands before the next NODE command appears are regarded as belonging to the node that has the NodeID specified by the NODE command.
    /// </summary>
    public class SbcNode : SbcCommand
    {
        public SbcNode(byte nodeId, bool isVisible)
        {
            Params.Add(nodeId);
            Params.Add((byte) (isVisible ? 1 : 0));
        }
        public SbcNode(EndianBinaryReader er)
        {
            Params.Add(er.ReadByte());
            Params.Add(er.ReadByte());
        }

        public override Commands Type => Commands.Node;

        public byte NodeId => Params[0];
        public bool IsVisible => Params[1] == 1;
    }

    /// <summary>
    /// Issues RestoreMtx command.
    /// Reads matrix from specified location of matrix stack of location coordinate matrix to current matrix.
    /// </summary>
    public class SbcMtx : SbcCommand
    {
        public SbcMtx(byte index)
        {
            Params.Add(index);
        }
        public SbcMtx(EndianBinaryReaderEx er)
        {
            Params.Add(er.ReadByte());
        }

        public override Commands Type => Commands.Mtx;
        public byte Index => Params[0];
    }

    public class SbcMat : SbcCommand
    {
        public SbcMat(byte option, byte matId)
        {
            Option = option;
            Params.Add(matId);
        }
        public SbcMat(byte option, EndianBinaryReaderEx er)
        {
            Option = option;
            Params.Add(er.ReadByte());
        }

        public override Commands Type => Commands.Mat;

        public byte MatId => Params[0];
    }

    public class SbcShape: SbcCommand
    {
        public SbcShape(byte shpId)
        {
            Params.Add(shpId);
        }
        public SbcShape(EndianBinaryReaderEx er)
        {
            Params.Add(er.ReadByte());
        }

        public override Commands Type => (Commands) 5; //LibNitro.G3D.Sbc.Commands.Shp;

        public byte ShpId => Params[0];

        public override string ToString() => "Shape";
    }

    public class SbcNodeDesc : SbcCommand
    {
        public SbcNodeDesc(byte option, byte nodeId, byte parentNodeId, byte ps, byte destIdx = 0, byte srcIdx = 0)
        {
            Option = option;
            Params.Add(nodeId); //Node ID
            Params.Add(parentNodeId); //Parent Node ID
            Params.Add(ps); //000000PS
            if (option == SbcFlg001 || option == SbcFlg011)
                Params.Add(destIdx); //Dest Idx
            if (option == SbcFlg010 || option == SbcFlg011)
                Params.Add(srcIdx); //Src Idx
        }
        public SbcNodeDesc(byte option, EndianBinaryReaderEx er)
        {
            Option = option;
            Params.Add(er.ReadByte()); //Node ID
            Params.Add(er.ReadByte()); //Parent Node ID
            Params.Add(er.ReadByte()); //000000PS
            if (option == SbcFlg001 || option == SbcFlg011)
                Params.Add(er.ReadByte()); //Dest Idx
            if (option == SbcFlg010 || option == SbcFlg011)
                Params.Add(er.ReadByte()); //Src Idx
        }
        public override Commands Type => Commands.Nodedesc;

        public byte NodeId => Params[0];
        public byte ParentNodeId => Params[1];
        public byte PS => Params[2];

        public byte? DestIdx
        {
            get
            {
                if (Option == SbcFlg001 || Option == SbcFlg011)
                {
                    return Params[3];
                }

                return null;
            }
        }

        public byte? SrcIdx
        {
            get
            {
                if (Option == SbcFlg011)
                {
                    return Params[4];
                }
                if (Option == SbcFlg010)
                {
                    return Params[3];
                }

                return null;
            }
        }
    }

    public class SbcBillboard : SbcCommand
    {
        public override Commands Type => Commands.BB;
        public SbcBillboard(byte option, byte nodeId, byte destIdx = 0, byte srcIdx = 0)
        {
            Option = option;
            Params.Add(nodeId); //Node ID
            if (option == SbcFlg001 || option == SbcFlg011)
                Params.Add(destIdx); //Dest Idx
            if (option == SbcFlg010 || option == SbcFlg011)
                Params.Add(srcIdx); //Src Idx
        }
        public SbcBillboard(byte option, EndianBinaryReaderEx er)
        {
            Option = option;
            Params.Add(er.ReadByte()); //Node ID
            if (option == SbcFlg001 || option == SbcFlg011)
                Params.Add(er.ReadByte()); //Dest Idx
            if (option == SbcFlg010 || option == SbcFlg011)
                Params.Add(er.ReadByte()); //Src Idx
        }

        public byte NodeId => Params[0];

        public byte? DestIdx
        {
            get
            {
                if (Option == SbcFlg001 || Option == SbcFlg011)
                {
                    return Params[1];
                }

                return null;
            }
        }

        public byte? SrcIdx
        {
            get
            {
                if (Option == SbcFlg011)
                {
                    return Params[2];
                }
                if (Option == SbcFlg010)
                {
                    return Params[1];
                }

                return null;
            }
        }
    }

    public class SbcBillboardY : SbcCommand
    {
        public override Commands Type => Commands.BBY;
        public SbcBillboardY(byte option, byte nodeId, byte destIdx = 0, byte srcIdx = 0)
        {
            Option = option;
            Params.Add(nodeId); //Node ID
            if (option == SbcFlg001 || option == SbcFlg011)
                Params.Add(destIdx); //Dest Idx
            if (option == SbcFlg010 || option == SbcFlg011)
                Params.Add(srcIdx); //Src Idx
        }
        public SbcBillboardY(byte option, EndianBinaryReaderEx er)
        {
            Option = option;
            Params.Add(er.ReadByte()); //Node ID
            if (option == SbcFlg001 || option == SbcFlg011)
                Params.Add(er.ReadByte()); //Dest Idx
            if (option == SbcFlg010 || option == SbcFlg011)
                Params.Add(er.ReadByte()); //Src Idx
        }

        public byte NodeId => Params[0];

        public byte? DestIdx
        {
            get
            {
                if (Option == SbcFlg001 || Option == SbcFlg011)
                {
                    return Params[1];
                }

                return null;
            }
        }

        public byte? SrcIdx
        {
            get
            {
                if (Option == SbcFlg011)
                {
                    return Params[2];
                }
                if (Option == SbcFlg010)
                {
                    return Params[1];
                }

                return null;
            }
        }
    }
    public class SbcNodeMix : SbcCommand
    {
        public class Item
        {
            public byte SrcIdx;
            public byte NodeId;
            public byte Ratio;
        }

        public SbcNodeMix(EndianBinaryReaderEx er)
        {
            Params.Add(er.ReadByte());
            var numMtx = er.ReadByte();
            Params.Add(numMtx);

            for (var i = 0; i < numMtx; i++)
            {
                Params.Add(er.ReadByte());
                Params.Add(er.ReadByte());
                Params.Add(er.ReadByte());
            }

            Params.Add(er.ReadByte());
        }
        public override Commands Type => Commands.Nodemix;

        public byte DestIdx => Params[0];
        public byte NumMtx => Params[1];

        public Item[] Items
        {
            get
            {
                var items = new Item[NumMtx];

                for (var i = 0; i < NumMtx; i++)
                {
                    items[i] = new Item
                    {
                        SrcIdx = Params[2 + (i * 3)],
                        NodeId = Params[2 + (i * 3 + 1)],
                        Ratio = Params[2 + (i * 3 + 2)]
                    };
                }

                return items;
            }
        }
    }

    public class SbcCallDl : SbcCommand
    {
        public SbcCallDl(byte relAddr, byte size)
        {
            Params.Add(relAddr);
            Params.Add(size);
        }
        public SbcCallDl(EndianBinaryReaderEx er)
        {
            Params.Add(er.ReadByte()); //Rel Addr
            Params.Add(er.ReadByte()); //Size
        }

        public override Commands Type => Commands.CallDl;

        public byte RelAddr => Params[0];
        public byte Size => Params[1];
    }

    public class SbcPosScale : SbcCommand
    {
        public SbcPosScale(byte option)
        {
            Option = option;
        }
        public SbcPosScale(byte option, EndianBinaryReaderEx er)
        {
            Option = option;
        }
        public override Commands Type => Commands.Posscale;
    }

    public class SbcEnvMap : SbcCommand
    {
        public SbcEnvMap(byte matId, byte flag = 0)
        {
            Option = 0;
            Params.Add(matId);
            Params.Add(flag);
        }
        public SbcEnvMap(EndianBinaryReaderEx er)
        {
            Option = 0;
            Params.Add(er.ReadByte());
            Params.Add(er.ReadByte());
        }
        public override Commands Type => Commands.Envmap;

        public byte MatId => Params[0];
        public byte Flag => Params[1];
    }

    public class SbcPrjMap : SbcCommand
    {
        public SbcPrjMap(byte matId, byte flag = 0)
        {
            Option = 0;
            Params.Add(matId);
            Params.Add(flag);
        }
        public SbcPrjMap(EndianBinaryReaderEx er)
        {
            Option = 0;
            Params.Add(er.ReadByte());
            Params.Add(er.ReadByte());
        }
        public override Commands Type => Commands.Prjmap;

        public byte MatId => Params[0];
        public byte Flag => Params[1];
    }
}