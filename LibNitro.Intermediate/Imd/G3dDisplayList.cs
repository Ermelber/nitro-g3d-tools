﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using LibEndianBinaryIO;
using LibNitro.GFX;

namespace LibNitro.Intermediate.Imd
{
    //Warning: This only works for NSBMD Display Lists (Only imd primitive array commands are implemented)

    public class DisplayListCommand
    {
        public DisplayListCommand() { }

        public DisplayListCommand(G3dCommand cmd)
        {
            G3dCommand = cmd;
        }

        public G3dCommand G3dCommand;

        public uint[] IntArgs;
        public float[] RealArgs;

        //Pretty print
        public override string ToString()
        {
            var s = $"{G3dCommand}";

            if (RealArgs != null)
            {
                foreach (var num in RealArgs)
                    s += $" {num}";
            }
            else if (IntArgs != null)
            {
                if (G3dCommand == G3dCommand.Begin)
                {
                    s += $" {(G3dPrimitiveType)IntArgs[0]}";
                }
                else
                {
                    foreach (var num in IntArgs)
                        s += $" {num}";
                }
            }

            return s;
        }
    }

    public enum G3dCommand : byte
    {
        Nop = 0x00, 

        MatrixMode = 0x10, 
        PushMatrix = 0x11, 
        PopMatrix = 0x12, 
        StoreMatrix = 0x13, 
        RestoreMatrix = 0x14, 
        Identity = 0x15, 
        LoadMatrix44 = 0x16, 
        LoadMatrix43 = 0x17, 
        MultMatrix44 = 0x18, 
        MultMatrix43 = 0x19, 
        MultMatrix33 = 0x1a, 
        Scale = 0x1b, 
        Translate = 0x1c, 

        Color = 0x20, 
        Normal = 0x21, 
        TexCoord = 0x22, 
        Vertex = 0x23, 
        VertexShort = 0x24, 
        VertexXY = 0x25, 
        VertexXZ = 0x26, 
        VertexYZ = 0x27, 
        VertexDiff = 0x28, 
        PolygonAttr = 0x29, 
        TexImageParam = 0x2a, 
        TexPlttBase = 0x2b, 

        MaterialColor0 = 0x30, 
        MaterialColor1 = 0x31, 
        LightVector = 0x32, 
        LightColor = 0x33, 
        Shininess = 0x34, 

        Begin = 0x40, 
        End = 0x41, 

        SwapBuffers = 0x50, 

        Viewport = 0x60, 

        BoxTest = 0x70, 
        PositionTest = 0x71, 
        VectorTest = 0x72, 

        Dummy = 0xFF
    }

    public enum G3dPrimitiveType : uint
    {
        Triangle = 0,
        Quadrilateral = 1,
        TriangleStrip = 2,
        QuadrilateralStrip = 3
    }

    public class G3dDisplayList
    {
        public static G3dPrimitiveType GetPrimitiveType(string type)
        {
            switch (type)
            {
                default:
                case "triangles": return G3dPrimitiveType.Triangle;
                case "quads": return G3dPrimitiveType.Quadrilateral;
                case "triangle_strip": return G3dPrimitiveType.TriangleStrip;
                case "quad_strip": return G3dPrimitiveType.QuadrilateralStrip;
            }
        }

        public static byte[] Encode(IEnumerable<DisplayListCommand> commands)
        {
            var m = new MemoryStream();
            var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);

            int offset = 0;
            int packed = 0;

            var cmdList = commands.ToList();
            
            var nops = (cmdList.Count % 4) == 0 ? 0 : 4 - (cmdList.Count % 4);

            //Add NOPs at the end
            for (int i = 0; i < nops; i++)
            {
                cmdList.Add(new DisplayListCommand(G3dCommand.Nop));
            }

            var commandQueue = new Queue<DisplayListCommand>();

            bool param0Flag = false;

            void Flush()
            {
                packed = 0;

                var count = commandQueue.Count;

                for (int i = 0; i < count; i++)
                {
                    var cmd = commandQueue.Dequeue();

                    //m.Position = offset;

                    switch (cmd.G3dCommand)
                    {
                        default:
                        case G3dCommand.Identity:
                        case G3dCommand.PushMatrix:
                        case G3dCommand.End:
                        case G3dCommand.Nop:
                            break;
                        case G3dCommand.TexCoord:
                            ew.WriteFixedPoint(cmd.RealArgs[0], true, 11, 4);
                            ew.WriteFixedPoint(cmd.RealArgs[1], true, 11, 4);
                            offset += 4;
                            break;
                        case G3dCommand.VertexXY:
                        case G3dCommand.VertexXZ:
                        case G3dCommand.VertexYZ:
                            ew.WriteFx16s(cmd.RealArgs);
                            offset += 4;
                            break;
                        case G3dCommand.Begin:
                        case G3dCommand.RestoreMatrix:
                            ew.Write(cmd.IntArgs[0]);
                            offset += 4;
                            break;
                        case G3dCommand.VertexDiff:
                            var vec2 = VecFx10.FromVector3((cmd.RealArgs[0] * 8, cmd.RealArgs[1] * 8,
                                cmd.RealArgs[2] * 8));
                            ew.Write(vec2);
                            offset += 4;
                            break;
                        case G3dCommand.VertexShort:
                            var vec3 = VecFx10.FromVector3((cmd.RealArgs[0], cmd.RealArgs[1], cmd.RealArgs[2]), 3,
                                6);
                            ew.Write(vec3);
                            offset += 4;
                            break;
                        case G3dCommand.Normal:
                            var vec = VecFx10.FromVector3((cmd.RealArgs[0], cmd.RealArgs[1], cmd.RealArgs[2]));
                            ew.Write(vec);
                            offset += 4;
                            break;
                        case G3dCommand.Color:
                            var color = GFXUtil.ConvertColorFormat(
                                (uint)Color.FromArgb(0, (int)cmd.IntArgs[0], (int)cmd.IntArgs[1],
                                    (int)cmd.IntArgs[2]).ToArgb(),
                                ColorFormat.ARGB8888, ColorFormat.ABGR1555);
                            ew.Write(color);
                            offset += 4;
                            break;
                        case G3dCommand.Vertex:
                            ew.WriteFx16s(cmd.RealArgs);
                            ew.Write((ushort)0);
                            offset += 8;
                            break;
                    }
                }

                if (param0Flag)
                {
                    ew.Write(0);
                    offset += 4;

                    param0Flag = false;
                }
            }

            foreach (var command in cmdList)
            {
                if (command.G3dCommand != G3dCommand.Nop)
                {
                    param0Flag = packed > 0 && (command.G3dCommand == G3dCommand.Identity ||
                                                command.G3dCommand == G3dCommand.PushMatrix ||
                                                command.G3dCommand == G3dCommand.End);
                }

                //m.Position = offset;

                commandQueue.Enqueue(command);
                ew.Write((byte) command.G3dCommand);
                packed++;
                offset++;

                if (packed == 4)
                {
                    Flush();
                }
            }
            
            var dl = m.ToArray();

            m.Close();

            return dl;
        }
        
        public static List<DisplayListCommand> Decode(byte[] dl)
        {
            var m = new MemoryStream(dl);
            var er = new EndianBinaryReaderEx(m, Endianness.LittleEndian);
            
            var commandQueue = new Queue<G3dCommand>();

            var decoded = new List<DisplayListCommand>();

            while (m.Position < dl.Length)
            {
                commandQueue.Enqueue((G3dCommand)er.ReadByte());
                commandQueue.Enqueue((G3dCommand)er.ReadByte());
                commandQueue.Enqueue((G3dCommand)er.ReadByte());
                commandQueue.Enqueue((G3dCommand)er.ReadByte());
                
                //Decode the 4 enqueued commands
                for (var j = 0; j < 4; j++)
                {
                    var cmd = commandQueue.Dequeue();

                    switch (cmd)
                    {
                        default:
                        case G3dCommand.PushMatrix:
                        case G3dCommand.End:
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd });
                            break;
                        case G3dCommand.TexCoord:
                            decoded.Add(new DisplayListCommand
                            {
                                G3dCommand = cmd,
                                RealArgs = new[]
                                {
                                    er.ReadFixedPoint(true, 11, 4),
                                    er.ReadFixedPoint(true, 11, 4)
                                }
                            });
                            break;
                        case G3dCommand.VertexXY:
                        case G3dCommand.VertexXZ:
                        case G3dCommand.VertexYZ:
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd, RealArgs = er.ReadFx16s(2) });
                            break;
                        case G3dCommand.Begin:
                        case G3dCommand.RestoreMatrix:
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd, IntArgs = er.ReadUInt32s(1) });
                            break;
                        case G3dCommand.VertexDiff:
                            var vec2 = VecFx10.ToVector3(er.ReadUInt32()) / 8;
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd, RealArgs = new[] { vec2.X, vec2.Y, vec2.Z } });
                            break;
                        case G3dCommand.VertexShort:
                            var vec3 = VecFx10.ToVector3(er.ReadUInt32(), 3, 6);
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd, RealArgs = new[] { vec3.X, vec3.Y, vec3.Z } });
                            break;
                        case G3dCommand.Normal:
                            var vec = VecFx10.ToVector3(er.ReadUInt32());
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd, RealArgs = new[] { vec.X, vec.Y, vec.Z } });
                            break;
                        case G3dCommand.Color:
                            var color = Color.FromArgb((int)GFXUtil.ConvertColorFormat(er.ReadUInt32(),
                                ColorFormat.ABGR1555, ColorFormat.ARGB8888));
                            decoded.Add(new DisplayListCommand { G3dCommand = cmd, IntArgs = new uint[] { color.R, color.G, color.B } });
                            break;
                        case G3dCommand.Vertex:
                            decoded.Add(new DisplayListCommand
                            {
                                G3dCommand = cmd,
                                RealArgs = er.ReadFx16s(3)
                            });
                            er.ReadUInt16();
                            break;
                    }
                }
            }

            m.Close();

            return decoded;
        }
    }
}
