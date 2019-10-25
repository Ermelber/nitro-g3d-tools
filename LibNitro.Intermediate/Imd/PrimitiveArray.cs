using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using LibFoundation.Math;

namespace LibNitro.Intermediate.Imd
{
    public class PrimitiveArray
    {
        [XmlElement("primitive")] public List<Primitive> Primitives = new List<Primitive>();

        [XmlAttribute("size")]
        public int Size
        {
            get => Primitives?.Count ?? 0;
            set { }
        }

        //Todo: Fix matrix thing
        public List<DisplayListCommand> GetDecodedCommands()
        {
            var decoded = new List<DisplayListCommand>();

            foreach (var primitive in Primitives)
                decoded.AddRange(primitive.GetDecodedCommands());

            //Remove first matrix command or something, idk how this shit works yet
            for (int i = 0; i < decoded.Count; i++)
            {
                if (decoded[i].G3dCommand == G3dCommand.RestoreMatrix)
                {
                    decoded.Remove(decoded[i]);
                    break;
                }
            }

            return decoded;
        }
    }

    public class Primitive
    {
        [XmlElement("mtx", typeof(MatrixCommand))]
        [XmlElement("pos_xy", typeof(PosXyCommand))]
        [XmlElement("pos_xz", typeof(PosXzCommand))]
        [XmlElement("pos_yz", typeof(PosYzCommand))]
        [XmlElement("pos_diff", typeof(PosDiffCommand))]
        [XmlElement("pos_xyz", typeof(PosXyzCommand))]
        [XmlElement("pos_s", typeof(PosShortCommand))]
        [XmlElement("clr", typeof(ColorCommand))]
        [XmlElement("nrm", typeof(NormalCommand))]
        [XmlElement("tex", typeof(TextureCoordCommand))]
        public List<PrimitiveCommand> Commands = new List<PrimitiveCommand>();

        [XmlAttribute("index")] public int Index;

        [XmlAttribute("type")] public string Type;

        [XmlAttribute("vertex_size")] public int VertexSize;

        public List<DisplayListCommand> GetDecodedCommands()
        {
            var decoded = new List<DisplayListCommand>();

            decoded.Add(new DisplayListCommand
            {
                G3dCommand = G3dCommand.Begin,
                IntArgs = new[] { (uint)G3dDisplayList.GetPrimitiveType(Type) }
            });

            foreach (var command in Commands)
            {
                if (command.G3dCommand == G3dCommand.RestoreMatrix) //todo: check if it's right
                {
                    if (command.GetDecodedCommand().IntArgs[0] != 0)
                        decoded.Add(command.GetDecodedCommand());
                }
                else
                    decoded.Add(command.GetDecodedCommand());
            }

            decoded.Add(new DisplayListCommand
            {
                G3dCommand = G3dCommand.End
            });

            return decoded;
        }
    }

    public abstract class PrimitiveCommand
    {
        public virtual G3dCommand G3dCommand => G3dCommand.Dummy;

        internal PrimitiveCommand()
        {
        }

        public abstract DisplayListCommand GetDecodedCommand();
    }

    public class TextureCoordCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.TexCoord;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {TexCoords.X, TexCoords.Y}
            };

        public TextureCoordCommand()
        {
        }

        public TextureCoordCommand(double s, double t)
        {
            St = $"{s} {t}";
        }

        public TextureCoordCommand(double u, double v, int texWidth, int texHeight)
        {
            St = $"{(u * texWidth)} {(v * -texHeight + texHeight)}";
        }

        [XmlAttribute("st")] public string St;

        [XmlIgnore]
        public Vector2 TexCoords => new Vector2(float.Parse(St.Split(' ')[0]), float.Parse(St.Split(' ')[1]));
    }

    public class ColorCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.Color;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                IntArgs = new[]
                {
                    uint.Parse(Rgb.Split(' ')[0]) * 8, uint.Parse(Rgb.Split(' ')[1]) * 8,
                    uint.Parse(Rgb.Split(' ')[2]) * 8
                }
            };

        public ColorCommand()
        {
        }

        public ColorCommand(float r, float g, float b)
        {
            Rgb = $"{(int) (r * 31)} {(int) (g * 31)} {(int) (b * 31)}";
        }

        public ColorCommand(int r, int g, int b)
        {
            Rgb = $"{r} {g} {b}";
        }

        [XmlAttribute("rgb")] public string Rgb;
    }

    public class MatrixCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.RestoreMatrix;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                IntArgs    = new[] {(uint) Index}
            };

        [XmlAttribute("idx")] public byte Index;
    }

    public class NormalCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.Normal;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Normal.X, Normal.Y, Normal.Z}
            };

        public NormalCommand()
        {
        }

        public NormalCommand(Vector3 normal)
        {
            //Clamp values first
            normal.X = normal.X > 0.998047f ? 0.998047f : normal.X < -1 ? -1 : normal.X;
            normal.Y = normal.Y > 0.998047f ? 0.998047f : normal.Y < -1 ? -1 : normal.Y;
            normal.Z = normal.Z > 0.998047f ? 0.998047f : normal.Z < -1 ? -1 : normal.Z;

            Xyz = $"{normal.X} {normal.Y} {normal.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;

        [XmlIgnore]
        public Vector3 Normal => (
            float.Parse(Xyz.Split(' ')[0]),
            float.Parse(Xyz.Split(' ')[1]),
            float.Parse(Xyz.Split(' ')[2]));
    }

    public class PosXyCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.VertexXY;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Position.X, Position.Y}
            };

        public PosXyCommand()
        {
        }

        public PosXyCommand(Vector3 vertex)
        {
            Xy = $"{vertex.X} {vertex.Y}";
        }


        [XmlAttribute("xy")] public string Xy;

        [XmlIgnore]
        public Vector2 Position => new Vector2(float.Parse(Xy.Split(' ')[0]), float.Parse(Xy.Split(' ')[1]));
    }

    public class PosXzCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.VertexXZ;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Position.X, Position.Y}
            };

        public PosXzCommand()
        {
        }

        public PosXzCommand(Vector3 vertex)
        {
            Xz = $"{vertex.X} {vertex.Z}";
        }

        [XmlAttribute("xz")] public string Xz;

        [XmlIgnore]
        public Vector2 Position => new Vector2(float.Parse(Xz.Split(' ')[0]), float.Parse(Xz.Split(' ')[1]));
    }

    public class PosYzCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.VertexYZ;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Position.X, Position.Y}
            };

        public PosYzCommand()
        {
        }

        public PosYzCommand(Vector3 vertex)
        {
            Yz = $"{vertex.Y} {vertex.Z}";
        }

        [XmlAttribute("yz")] public string Yz;

        [XmlIgnore]
        public Vector2 Position => new Vector2(float.Parse(Yz.Split(' ')[0]), float.Parse(Yz.Split(' ')[1]));
    }

    public class PosDiffCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.VertexDiff;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Diff.X, Diff.Y, Diff.Z}
            };

        public PosDiffCommand()
        {
        }

        public PosDiffCommand(Vector3 diff)
        {
            Xyz = $"{diff.X} {diff.Y} {diff.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;

        [XmlIgnore]
        public Vector3 Diff => (
            float.Parse(Xyz.Split(' ')[0]),
            float.Parse(Xyz.Split(' ')[1]),
            float.Parse(Xyz.Split(' ')[2]));
    }

    public class PosXyzCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.Vertex;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Position.X, Position.Y, Position.Z}
            };

        public PosXyzCommand()
        {
        }

        public PosXyzCommand(Vector3 vertex)
        {
            Xyz = $"{vertex.X} {vertex.Y} {vertex.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;

        [XmlIgnore]
        public Vector3 Position => (
            float.Parse(Xyz.Split(' ')[0]),
            float.Parse(Xyz.Split(' ')[1]),
            float.Parse(Xyz.Split(' ')[2]));
    }

    public class PosShortCommand : PrimitiveCommand
    {
        public override G3dCommand G3dCommand => G3dCommand.VertexShort;

        public override DisplayListCommand GetDecodedCommand() =>
            new DisplayListCommand
            {
                G3dCommand = G3dCommand,
                RealArgs   = new[] {Position.X, Position.Y, Position.Z}
            };

        public PosShortCommand()
        {
        }

        public PosShortCommand(Vector3 vertex)
        {
            Xyz = $"{vertex.X} {vertex.Y} {vertex.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;

        [XmlIgnore]
        public Vector3 Position => (
            float.Parse(Xyz.Split(' ')[0]),
            float.Parse(Xyz.Split(' ')[1]),
            float.Parse(Xyz.Split(' ')[2]));
    }
}