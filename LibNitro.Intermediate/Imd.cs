using System.Collections.Generic;
using System.Xml.Serialization;
using LibFoundation.Math;

namespace LibNitro.Intermediate
{
    [XmlRoot("imd")]
    public class Imd : Intermediate<Imd, ImdBody>
    {
        public Imd()
        {
            Head.Title = "Model Data for NINTENDO NITRO-System";
            Body = new ImdBody
            {
                OriginalCreateInfo = Head.CreateInfo
            };
        }
    }

    public class ImdBody : IntermediateBody
    {
        [XmlElement("original_create")] public CreateInfo OriginalCreateInfo;

        [XmlElement("original_generator")] public GeneratorInfo OriginalGeneratorInfo;

        [XmlElement("model_info")] public ModelInfo ModelInfo = new ModelInfo();

        [XmlElement("box_test")] public BoxTest BoxTest = new BoxTest();

        [XmlElement("tex_image_array")] public TexImageArray TexImageArray;

        [XmlElement("tex_palette_array")] public TexPaletteArray TexPaletteArray;

        [XmlElement("material_array")] public MaterialArray MaterialArray = new MaterialArray();

        [XmlElement("matrix_array")] public MatrixArray MatrixArray = new MatrixArray();

        [XmlElement("polygon_array")] public PolygonArray PolygonArray = new PolygonArray();

        [XmlElement("node_array")] public NodeArray NodeArray = new NodeArray();

        [XmlElement("output_info")] public OutputInfo OutputInfo = new OutputInfo();
    }

    public class ModelInfo
    {
        [XmlAttribute("pos_scale")] public int PosScale;

        [XmlAttribute("scaling_rule")] public string ScalingRule = "standard";

        [XmlAttribute("vertex_style")] public string VertexStyle = "direct";

        [XmlAttribute("magnify")] public float Magnify = 1;

        [XmlAttribute("tool_start_frame")] public byte ToolStartFrame = 1;

        [XmlAttribute("tex_matrix_mode")] public string TexMatrixMode = "maya";

        [XmlAttribute("compress_node")] public string CompressNode = "unite_combine";

        [XmlAttribute("node_size")] public string NodeSize = "1 1";

        [XmlAttribute("compress_material")] public string CompressMaterial = "on";

        [XmlAttribute("material_size")] public string MaterialSize;

        [XmlAttribute("output_texture")] public string OutputTexture = "used";

        [XmlAttribute("force_full_weight")] public string ForceFullWeight = "on";

        [XmlAttribute("use_primitive_strip")] public string UsePrimitiveStrip = "off";
    }

    public class BoxTest
    {
        [XmlAttribute("pos_scale")] public int PosScale;

        [XmlAttribute("xyz")] public string Xyz;

        [XmlAttribute("whd")] public string Whd;
    }

    public class TexImageArray
    {
        [XmlElement("tex_image")] public List<TexImage> TexImages;

        public int GetIndexByName(string name)
        {
            for (int i = 0; i < TexImages.Count; i++)
            {
                if (name == TexImages[i].Name)
                    return i;
            }

            return -1;
        }

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (TexImages != null)
                    return TexImages.Count;
                return 0;
            }

            set { }
        }
    }

    public class TexImage
    {
        [XmlElement("bitmap")] public TexBitmap Bitmap;

        [XmlElement("tex4x4_palette_idx")] public Tex4x4PaletteIndex Tex4x4PaletteIndex;

        [XmlAttribute("index")] public int Index;

        [XmlAttribute("name")] public string Name;

        [XmlAttribute("width")] public int Width;

        [XmlAttribute("height")] public int Height;

        [XmlAttribute("original_width")] public int OriginalWidth;

        [XmlAttribute("original_height")] public int OriginalHeight;

        [XmlAttribute("format")] public string Format;

        [XmlAttribute("color0_mode")] public string Color0Mode;

        [XmlAttribute("palette_name")] public string PaletteName;

        [XmlAttribute("path")] public string Path;
    }

    public class TexBitmap
    {
        [XmlAttribute("size")] public int Size;

        [XmlText] public string Value;
    }

    public class Tex4x4PaletteIndex
    {
        [XmlAttribute("size")] public int Size;

        [XmlText] public string Value;
    }

    public class TexPaletteArray
    {
        [XmlElement("tex_palette")] public List<TexPalette> TexPalettes;

        public int GetIndexByName(string name)
        {
            for (int i = 0; i < TexPalettes.Count; i++)
            {
                if (name == TexPalettes[i].Name)
                    return i;
            }

            return -1;
        }

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (TexPalettes != null)
                    return TexPalettes.Count;
                return 0;
            }

            set { }
        }
    }

    public class TexPalette
    {
        [XmlAttribute("index")] public int Index;

        [XmlAttribute("name")] public string Name;

        [XmlAttribute("color_size")] public int ColorSize;

        [XmlText] public string Value;
    }

    public class MaterialArray
    {
        [XmlElement("material")] public List<Material> Materials = new List<Material>();

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (Materials != null)
                    return Materials.Count;
                return 0;
            }

            set { }
        }
    }

    public class Material
    {
        [XmlAttribute("index")] public int Index;

        [XmlAttribute("name")] public string Name;

        [XmlAttribute("light0")] public string Light0 = "off";

        [XmlAttribute("light1")] public string Light1 = "off";

        [XmlAttribute("light2")] public string Light2 = "off";

        [XmlAttribute("light3")] public string Light3 = "off";

        [XmlAttribute("face")] public string Face = "front";

        [XmlAttribute("alpha")] public byte Alpha = 31;

        [XmlAttribute("wire_mode")] public string WireMode = "off";

        [XmlAttribute("polygon_mode")] public string PolygonMode = "modulate";

        [XmlAttribute("polygon_id")] public byte PolygonId;

        [XmlAttribute("fog_flag")] public string FogFlag = "off";

        [XmlAttribute("depth_test_decal")] public string DepthTestDecal = "off";

        [XmlAttribute("translucent_update_depth")] public string TranslucentUpdateDepth = "off";

        [XmlAttribute("render_1_pixel")] public string Render1Pixel = "off";

        [XmlAttribute("far_clipping")] public string FarClipping = "off";

        [XmlAttribute("diffuse")] public string Diffuse = "31 31 31";

        [XmlAttribute("ambient")] public string Ambient = "31 31 31";

        [XmlAttribute("specular")] public string Specular = "0 0 0";

        [XmlAttribute("emission")] public string Emission = "0 0 0";

        [XmlAttribute("shininess_table_flag")] public string ShininessTableFlag = "off";

        [XmlAttribute("tex_image_idx")] public int TexImageIdx = -1;

        [XmlAttribute("tex_palette_idx")] public int TexPaletteIdx = -1;

        [XmlAttribute("tex_tiling")] public string TexTiling;

        [XmlAttribute("tex_scale")] public string TexScale;

        [XmlAttribute("tex_rotate")] public string TexRotate;

        [XmlAttribute("tex_translate")] public string TexTranslate;

        [XmlAttribute("tex_gen_mode")] public string TexGenMode;
    }

    public class MatrixArray
    {
        [XmlElement("matrix")] public List<Matrix> Matrices = new List<Matrix>();

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (Matrices != null)
                    return Matrices.Count;
                return 0;
            }

            set { }
        }
    }

    public class Matrix
    {
        [XmlAttribute("index")] public int Index;

        [XmlAttribute("mtx_weight")] public byte MatrixWeight;

        [XmlAttribute("node_idx")] public byte NodeIndex;
    }

    public class PolygonArray
    {
        [XmlElement("polygon")] public List<Polygon> Polygons = new List<Polygon>();

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (Polygons != null)
                    return Polygons.Count;
                return 0;
            }

            set { }
        }
    }

    public class Polygon
    {
        [XmlElement("mtx_prim")] public List<MatrixPrimitive> MatrixPrimitives = new List<MatrixPrimitive>();

        [XmlAttribute("index")] public int Index;

        [XmlAttribute("name")] public string Name;

        [XmlAttribute("vertex_size")] public int VertexSize;

        [XmlAttribute("polygon_size")] public int PolygonSize;

        [XmlAttribute("triangle_size")] public int TriangleSize;

        [XmlAttribute("quad_size")] public int QuadSize;

        [XmlAttribute("volume_min")] public string VolumeMin;

        [XmlAttribute("volume_max")] public string VolumeMax;

        [XmlAttribute("volume_r")] public decimal VolumeR;

        [XmlAttribute("mtx_prim_size")]
        public int MatrixPrimitivesSize
        {
            get
            {
                if (MatrixPrimitives != null)
                    return MatrixPrimitives.Count;
                return 0;
            }
            set { }
        }

        [XmlAttribute("nrm_flag")] public string NrmFlag = "off";

        [XmlAttribute("clr_flag")] public string ClrFlag = "off";

        [XmlAttribute("tex_flag")] public string TexFlag = "on";
    }

    public class MatrixPrimitive
    {
        [XmlElement("mtx_list")]
        public MatrixList MtxList = new MatrixList();

        [XmlElement("primitive_array")]
        public PrimitiveArray PrimitiveArray;

        [XmlAttribute("index")] public int Index;
    }

    public class MatrixList
    {
        [XmlAttribute("size")] public int Size = 1;

        [XmlText] public string Value = "0";
    }

    public class PrimitiveArray
    {
        [XmlElement("primitive")] public List<Primitive> Primitives = new List<Primitive>();

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (Primitives != null)
                    return Primitives.Count;
                return 0;
            }

            set { }
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
    }

    public abstract class PrimitiveCommand
    {
        internal PrimitiveCommand() { }
    }

    public class TextureCoordCommand : PrimitiveCommand
    {
        public TextureCoordCommand() { }

        public TextureCoordCommand(double s, double t)
        {
            St = $"{s} {t}";
        }

        public TextureCoordCommand(double u, double v, int texWidth, int texHeight)
        {
            St = $"{(u * texWidth)} {(v * -texHeight + texHeight)}";
        }

        [XmlAttribute("st")] public string St;
    }

    public class ColorCommand : PrimitiveCommand
    {
        public ColorCommand()
        {
        }

        public ColorCommand(float r, float g, float b)
        {
            Rgb = $"{(int)(r * 31)} {(int)(g * 31)} {(int)(b * 31)}";
        }

        [XmlAttribute("rgb")] public string Rgb;
    }

    public class MatrixCommand : PrimitiveCommand
    {
        [XmlAttribute("idx")] public byte Index;
    }

    public class NormalCommand : PrimitiveCommand
    {
        public NormalCommand() { }

        public NormalCommand(Vector3 normal)
        {
            //Clamp values first
            normal.X = normal.X > 0.998047f ? 0.998047f : normal.X < -1 ? -1 : normal.X;
            normal.Y = normal.Y > 0.998047f ? 0.998047f : normal.Y < -1 ? -1 : normal.Y;
            normal.Z = normal.Z > 0.998047f ? 0.998047f : normal.Z < -1 ? -1 : normal.Z;

            Xyz = $"{normal.X} {normal.Y} {normal.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;
    }

    public class PosXyCommand : PrimitiveCommand
    {
        public PosXyCommand() { }

        public PosXyCommand(Vector3 vertex)
        {
            Xy = $"{vertex.X} {vertex.Y}";
        }


        [XmlAttribute("xy")] public string Xy;
    }

    public class PosXzCommand : PrimitiveCommand
    {
        public PosXzCommand() { }

        public PosXzCommand(Vector3 vertex)
        {
            Xz = $"{vertex.X} {vertex.Z}";
        }

        [XmlAttribute("xz")] public string Xz;
    }

    public class PosYzCommand : PrimitiveCommand
    {
        public PosYzCommand() { }

        public PosYzCommand(Vector3 vertex)
        {
            Yz = $"{vertex.Y} {vertex.Z}";
        }

        [XmlAttribute("yz")] public string Yz;
    }

    public class PosDiffCommand : PrimitiveCommand
    {
        public PosDiffCommand() { }

        public PosDiffCommand(Vector3 diff)
        {
            Xyz = $"{diff.X} {diff.Y} {diff.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;
    }

    public class PosXyzCommand : PrimitiveCommand
    {
        public PosXyzCommand() { }

        public PosXyzCommand(Vector3 vertex)
        {
            Xyz = $"{vertex.X} {vertex.Y} {vertex.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;
    }

    public class PosShortCommand : PrimitiveCommand
    {
        public PosShortCommand() { }

        public PosShortCommand(Vector3 vertex)
        {
            Xyz = $"{vertex.X} {vertex.Y} {vertex.Z}";
        }

        [XmlAttribute("xyz")] public string Xyz;
    }

    public class NodeArray
    {
        [XmlElement("node")]
        public List<Node> Nodes = new List<Node>();

        [XmlAttribute("size")]
        public int Size
        {
            get
            {
                if (Nodes != null)
                    return Nodes.Count;
                return 0;
            }

            set { }
        }
    }

    public class Node
    {
        [XmlElement("display")]
        public List<NodeDisplay> Displays = new List<NodeDisplay>();

        [XmlAttribute("index")] public int Index;

        [XmlAttribute("name")] public string Name;

        [XmlAttribute("kind")] public string Kind;

        [XmlAttribute("parent")] public sbyte Parent = -1;

        [XmlAttribute("child")] public sbyte Child = -1;

        [XmlAttribute("brother_next")] public sbyte BrotherNext = -1;

        [XmlAttribute("brother_prev")] public sbyte BrotherPrev = -1;

        [XmlAttribute("draw_mtx")] public string DrawMtx = "on";

        [XmlAttribute("billboard")] public string Billboard = "off";

        [XmlAttribute("scale")] public string Scale = "1.000000 1.000000 1.000000";

        [XmlAttribute("rotate")] public string Rotate = "0.000000 0.000000 0.000000";

        [XmlAttribute("translate")] public string Translate = "0.000000 0.000000 0.000000";

        [XmlAttribute("visibility")] public string Visibility = "on";

        [XmlAttribute("display_size")]
        public int DisplaySize
        {
            get
            {
                if (Displays != null)
                    return Displays.Count;
                return 0;
            }

            set { }
        }

        [XmlAttribute("vertex_size")] public int VertexSize;

        [XmlAttribute("polygon_size")] public int PolygonSize;

        [XmlAttribute("triangle_size")] public int TriangleSize;

        [XmlAttribute("quad_size")] public int QuadSize;

        [XmlAttribute("volume_min")] public string VolumeMin;

        [XmlAttribute("volume_max")] public string VolumeMax;

        [XmlAttribute("volume_r")] public decimal VolumeR;
    }

    public class NodeDisplay
    {
        [XmlAttribute("index")] public int Index;

        [XmlAttribute("material")] public int Material;

        [XmlAttribute("polygon")] public int Polygon;

        [XmlAttribute("priority")] public int Priority;
    }

    public class OutputInfo
    {
        [XmlAttribute("vertex_size")] public int VertexSize;

        [XmlAttribute("polygon_size")] public int PolygonSize;

        [XmlAttribute("triangle_size")] public int TriangleSize;

        [XmlAttribute("quad_size")] public int QuadSize;
    }
}
