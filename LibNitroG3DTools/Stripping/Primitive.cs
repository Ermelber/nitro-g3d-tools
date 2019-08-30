using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibNitroG3DTools.Stripping
{
    public class Primitive : IComparable<Primitive>
    {
        public enum PrimitiveType
        {
            Triangles,
            Quads,
            TriangleStrip,
            QuadStrip,
            Other,
            BadAllocation
        }

        public Primitive()
        {
        }

        public Primitive(int vtxCount)
        {
            VertexCount = vtxCount;
            Matrices.AddRange(new int[vtxCount]);
            Positions.AddRange(new int[vtxCount]);
            Normals.AddRange(new int[vtxCount]);
            Colors.AddRange(new int[vtxCount]);
            TexCoords.AddRange(new int[vtxCount]);
        }

        public Primitive(Primitive original)
        {
            Type        = original.Type;
            VertexCount = original.VertexCount;
            Matrices.AddRange(original.Matrices);
            Positions.AddRange(original.Positions);
            Normals.AddRange(original.Normals);
            Colors.AddRange(original.Colors);
            TexCoords.AddRange(original.TexCoords);

            Processed          = original.Processed;
            NextCandidateCount = original.NextCandidateCount;
            NextCandidates[0]  = original.NextCandidates[0];
            NextCandidates[1]  = original.NextCandidates[1];
            NextCandidates[2]  = original.NextCandidates[2];
            NextCandidates[3]  = original.NextCandidates[3];
        }

        public PrimitiveType Type        { get; set; } = PrimitiveType.Other;
        public int           VertexCount { get; set; }
        public List<int>     Matrices    { get; } = new List<int>();
        public List<int>     Positions   { get; } = new List<int>();
        public List<int>     Normals     { get; } = new List<int>();
        public List<int>     Colors      { get; } = new List<int>();
        public List<int>     TexCoords   { get; } = new List<int>();

        //For stripping
        public bool  Processed          { get; set; }
        public int   NextCandidateCount { get; set; }
        public int[] NextCandidates     { get; } = new int[4] {-1, -1, -1, -1};

        public void AddVtx(Primitive src, int vtxIdx)
        {
            VertexCount++;
            Matrices.Add(src.Matrices[vtxIdx]);
            Positions.Add(src.Positions[vtxIdx]);
            Normals.Add(src.Normals[vtxIdx]);
            Colors.Add(src.Colors[vtxIdx]);
            TexCoords.Add(src.TexCoords[vtxIdx]);
        }

        public bool IsVtxExtraDataEqual(int vtxA, Primitive b, int vtxB)
        {
            return Matrices[vtxA] == b.Matrices[vtxB] &&
                   Normals[vtxA] == b.Normals[vtxB] &&
                   Colors[vtxA] == b.Colors[vtxB] &&
                   TexCoords[vtxA] == b.TexCoords[vtxB];
        }

        public bool IsSuitableNextTStripCandidate(Primitive candidate)
        {
            int equalCount = 0;
            int firstI     = 0;
            int firstJ     = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (Positions[i] != candidate.Positions[j] || !IsVtxExtraDataEqual(i, candidate, j))
                        continue;
                    if (equalCount == 0)
                    {
                        firstI = i;
                        firstJ = j;
                    }
                    else if (equalCount == 1)
                    {
                        if (firstI == 0 && i == 2)
                            return firstJ < j || (firstJ == 2 && j == 0);
                        return firstJ > j || (firstJ == 0 && j == 2);
                    }

                    equalCount++;
                }
            }

            return false;
        }

        public bool IsSuitableNextTStripCandidateWithEdge(Primitive candidate, int vtx0, int vtx1)
        {
            int equalCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (candidate.Positions[i] == Positions[vtx0] && IsVtxExtraDataEqual(vtx0, candidate, i))
                    equalCount++;
                if (candidate.Positions[i] == Positions[vtx1] && IsVtxExtraDataEqual(vtx1, candidate, i))
                    equalCount++;
            }

            return equalCount == 2;
        }

        public bool IsSuitableNextQStripCandidate(Primitive candidate)
        {
            int equalCount = 0;
            int firstI     = 0;
            int firstJ     = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (Positions[i] != candidate.Positions[j] || !IsVtxExtraDataEqual(i, candidate, j))
                        continue;
                    if (equalCount == 0)
                    {
                        firstI = i;
                        firstJ = j;
                    }
                    else if (equalCount == 1)
                    {
                        if (firstI == 0 && i == 3)
                            return firstJ < j || (firstJ == 3 && j == 0);
                        return firstJ > j || (firstJ == 0 && j == 3);
                    }

                    equalCount++;
                }
            }

            return false;
        }

        public bool IsSuitableNextQStripCandidateWithEdge(Primitive candidate, int vtx0, int vtx1)
        {
            int equalCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (candidate.Positions[i] == Positions[vtx0] && IsVtxExtraDataEqual(vtx0, candidate, i))
                    equalCount++;
                if (candidate.Positions[i] == Positions[vtx1] && IsVtxExtraDataEqual(vtx1, candidate, i))
                    equalCount++;
            }

            return equalCount == 2;
        }

        private static int GetPrimitiveTypePriority(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.QuadStrip:
                    return 0;
                case PrimitiveType.TriangleStrip:
                    return 1;
                case PrimitiveType.Quads:
                    return 2;
                case PrimitiveType.Triangles:
                    return 3;
                default:
                    return -1;
            }
        }

        public int CompareTo(Primitive other)
        {
            int aPrio = GetPrimitiveTypePriority(Type);
            int bPrio = GetPrimitiveTypePriority(other.Type);
            if (aPrio != bPrio)
                return aPrio.CompareTo(bPrio);
            return VertexCount.CompareTo(other.VertexCount);
        }
    }
}