using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibNitroG3DTools.Stripping
{
    public class TriStripper
    {
        public static (int a, int b) GetPreviousTriEdge(int a, int b)
        {
            if(b == 0)
                return (2 - (a == 1 ? 0 : 1), a);

            if (b == 1)
                return (a == 2 ? 0 : 2, a);

            return (a == 0 ? 1 : 0, a);
                
        }

        private static int TryStripInDirection(
            Primitive[] triList, int triIdx, int vtxA, int vtxB)
        {
            var processedTmp = triList.Select(a => a.Processed).ToArray();
            processedTmp[triIdx] = true;
            var tri = triList[triIdx];
            (vtxB, vtxA) = GetPreviousTriEdge(vtxB, vtxA);
            int triCount = 1;
            while (true)
            {
                int i;
                for (i = 0; i < 3; i++)
                {
                    if (tri.NextCandidates[i] == -1)
                        continue;
                    var candidate = triList[tri.NextCandidates[i]];
                    if (processedTmp[tri.NextCandidates[i]])
                        continue;
                    if (!tri.IsSuitableNextTStripCandidateWithEdge(candidate, vtxA, vtxB))
                        continue;
                    int posIdxA = tri.Positions[vtxA];
                    for (vtxA = 0; vtxA < 3; vtxA++)
                        if (candidate.Positions[vtxA] == posIdxA)
                            break;
                    int posIdxB = tri.Positions[vtxB];
                    for (vtxB = 0; vtxB < 3; vtxB++)
                        if (candidate.Positions[vtxB] == posIdxB)
                            break;
                    if (vtxA != 3 && vtxB != 3)
                    {
                        (vtxB, vtxA) = GetPreviousTriEdge(vtxB, vtxA);
                        processedTmp[tri.NextCandidates[i]] = true;
                        triCount++;
                        tri = candidate;
                        break;
                    }
                }

                if (i == 3)
                    break;
            }

            return triCount;
        }

        private static Primitive MakeTStripPrimitive(
            Primitive[] triList, int triIdx, int vtxA, int vtxB)
        {
            var result = new Primitive();
            result.Type = Primitive.PrimitiveType.TriangleStrip;
            var tri = triList[triIdx];
            tri.Processed = true;
            result.AddVtx(tri, vtxA);
            result.AddVtx(tri, vtxB);
            (vtxB, vtxA) = GetPreviousTriEdge(vtxB, vtxA);
            result.AddVtx(tri, vtxB);
            while (true)
            {
                int i;
                for (i = 0; i < 3; i++)
                {
                    if (tri.NextCandidates[i] == -1)
                        continue;
                    var candidate = triList[tri.NextCandidates[i]];
                    if (candidate.Processed)
                        continue;
                    if (!tri.IsSuitableNextTStripCandidateWithEdge(candidate, vtxA, vtxB))
                        continue;
                    int posIdxA = tri.Positions[vtxA];
                    for (vtxA = 0; vtxA < 3; vtxA++)
                        if (candidate.Positions[vtxA] == posIdxA)
                            break;
                    int posIdxB = tri.Positions[vtxB];
                    for (vtxB = 0; vtxB < 3; vtxB++)
                        if (candidate.Positions[vtxB] == posIdxB)
                            break;
                    if (vtxA != 3 && vtxB != 3)
                    {
                        (vtxB, vtxA) = GetPreviousTriEdge(vtxB, vtxA);
                        result.AddVtx(candidate, vtxB);
                        candidate.Processed = true;
                        tri = candidate;
                        break;
                    }
                }

                if (i == 3)
                    break;
            }

            return result;
        }

        public static Primitive[] Process(Primitive[] primitives)
        {
            var result = new List<Primitive>();
            var triList = primitives.Where(a => a.Type == Primitive.PrimitiveType.Triangles).ToArray();
            foreach (var tri in triList)
                tri.Processed = false;

            //Find candidates
            foreach (var tri in triList)
            {
                tri.NextCandidateCount = 0;
                tri.NextCandidates[0] = -1;
                tri.NextCandidates[1] = -1;
                tri.NextCandidates[2] = -1;
                tri.NextCandidates[3] = -1;
                for (int i = 0; i < triList.Length; i++)
                {
                    if (!tri.IsSuitableNextTStripCandidate(triList[i]))
                        continue;
                    tri.NextCandidates[tri.NextCandidateCount++] = i;
                    if (tri.NextCandidateCount >= 3)
                        break;
                }
            }

            //Main loop
            while (true)
            {
                int count = 0;
                foreach (var tri in triList.Where(a => !a.Processed))
                {
                    count++;
                    if (tri.NextCandidateCount > 0)
                        tri.NextCandidateCount = tri.NextCandidates.Count(a => a != -1 && !triList[a].Processed);
                }

                if (count == 0)
                    break;

                int minCandCountIdx = -1;
                int minCandCount = int.MaxValue;
                for (int i = 0; i < triList.Length; i++)
                {
                    if (triList[i].Processed)
                        continue;
                    if (triList[i].NextCandidateCount < minCandCount)
                    {
                        minCandCount = triList[i].NextCandidateCount;
                        minCandCountIdx = i;
                        if (minCandCount <= 1)
                            break;
                    }
                }

                int maxTris = 0;
                int maxTrisVtx0 = -1;
                int maxTrisVtx1 = -1;
                for (int i = 0; i < 3; i++)
                {
                    int vtx0 = i;
                    int vtx1 = i == 2 ? 0 : i + 1;
                    int triCount = TryStripInDirection(triList, minCandCountIdx, vtx0, vtx1);
                    if (triCount > maxTris)
                    {
                        maxTris = triCount;
                        maxTrisVtx0 = vtx0;
                        maxTrisVtx1 = vtx1;
                    }
                }

                if (maxTris <= 1)
                {
                    var tri = triList[minCandCountIdx];
                    tri.Processed = true;
                    result.Add(new Primitive(tri));
                }
                else
                    result.Add(MakeTStripPrimitive(triList, minCandCountIdx, maxTrisVtx0, maxTrisVtx1));
            }

            result.AddRange(primitives.Where(a => a.Type != Primitive.PrimitiveType.Triangles));
            return result.ToArray();
        }
    }
}
