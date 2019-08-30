using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibNitroG3DTools.Stripping
{
    public class QuadStripper
    {
        private static (int a, int b) GetOppositeQuadEdge(int a, int b)
        {
            if (a == 3 && b == 0)
                return (2, 1);

            if (a == 0 && b == 3)
                return (1, 2);

            if (a >= b)
                return (a == 3 ? 0 : a + 1, b == 0 ? 3 : b - 1);

            return (a == 0 ? 3 : a - 1, b == 3 ? 0 : b + 1);
        }

        private static int TryStripInDirection(
            Primitive[] quadList, int quadIdx, int vtxA, int vtxB)
        {
            var processedTmp = quadList.Select(a => a.Processed).ToArray();
            processedTmp[quadIdx] = true;
            var quad = quadList[quadIdx];
            (vtxA, vtxB) = GetOppositeQuadEdge(vtxA, vtxB);
            int quadCount = 1;
            while (true)
            {
                int i;
                for (i = 0; i < 4; i++)
                {
                    if (quad.NextCandidates[i] == -1)
                        continue;
                    var candidate = quadList[quad.NextCandidates[i]];
                    if (processedTmp[quad.NextCandidates[i]])
                        continue;
                    if (!quad.IsSuitableNextQStripCandidateWithEdge(candidate, vtxA, vtxB))
                        continue;
                    int posIdxA = quad.Positions[vtxA];
                    for (vtxA = 0; vtxA < 4; vtxA++)
                        if (candidate.Positions[vtxA] == posIdxA)
                            break;
                    int posIdxB = quad.Positions[vtxB];
                    for (vtxB = 0; vtxB < 4; vtxB++)
                        if (candidate.Positions[vtxB] == posIdxB)
                            break;
                    if (vtxA != 4 && vtxB != 4)
                    {
                        (vtxA, vtxB) = GetOppositeQuadEdge(vtxA, vtxB);
                        processedTmp[quad.NextCandidates[i]] = true;
                        quadCount++;
                        quad = candidate;
                        break;
                    }
                }

                if (i == 4 || quadCount >= 1706)
                    break;
            }

            return quadCount;
        }

        private static Primitive MakeQStripPrimitive(
            Primitive[] quadList, int quadIdx, int vtxA, int vtxB)
        {
            var result = new Primitive();
            result.Type = Primitive.PrimitiveType.QuadStrip;
            var quad = quadList[quadIdx];
            quad.Processed = true;
            result.AddVtx(quad, vtxA);
            result.AddVtx(quad, vtxB);
            (vtxA, vtxB) = GetOppositeQuadEdge(vtxA, vtxB);
            result.AddVtx(quad, vtxA);
            result.AddVtx(quad, vtxB);
            int quadCount = 1;
            while (true)
            {
                int i;
                for (i = 0; i < 4; i++)
                {
                    if (quad.NextCandidates[i] == -1)
                        continue;
                    var candidate = quadList[quad.NextCandidates[i]];
                    if (candidate.Processed)
                        continue;
                    if (!quad.IsSuitableNextQStripCandidateWithEdge(candidate, vtxA, vtxB))
                        continue;
                    int posIdxA = quad.Positions[vtxA];
                    for (vtxA = 0; vtxA < 4; vtxA++)
                        if (candidate.Positions[vtxA] == posIdxA)
                            break;
                    int posIdxB = quad.Positions[vtxB];
                    for (vtxB = 0; vtxB < 4; vtxB++)
                        if (candidate.Positions[vtxB] == posIdxB)
                            break;
                    if (vtxA != 4 && vtxB != 4)
                    {
                        (vtxA, vtxB) = GetOppositeQuadEdge(vtxA, vtxB);
                        result.AddVtx(candidate, vtxA);
                        result.AddVtx(candidate, vtxB);
                        candidate.Processed = true;
                        quadCount++;
                        quad = candidate;
                        break;
                    }
                }

                if (i == 4 || quadCount >= 1706)
                    break;
            }

            return result;
        }

        public static Primitive[] Process(Primitive[] primitives)
        {
            var result   = new List<Primitive>();
            var quadList = primitives.Where(a => a.Type == Primitive.PrimitiveType.Quads).ToArray();
            foreach (var quad in quadList)
                quad.Processed = false;

            //Find candidates
            foreach (var quad in quadList)
            {
                quad.NextCandidateCount = 0;
                quad.NextCandidates[0]  = -1;
                quad.NextCandidates[1]  = -1;
                quad.NextCandidates[2]  = -1;
                quad.NextCandidates[3]  = -1;
                for (int i = 0; i < quadList.Length; i++)
                {
                    if (!quad.IsSuitableNextQStripCandidate(quadList[i]))
                        continue;
                    quad.NextCandidates[quad.NextCandidateCount++] = i;
                    if (quad.NextCandidateCount >= 4)
                        break;
                }
            }

            //Main loop
            while (true)
            {
                int count = 0;
                foreach (var quad in quadList.Where(a => !a.Processed))
                {
                    count++;
                    if (quad.NextCandidateCount > 0)
                        quad.NextCandidateCount = quad.NextCandidates.Count(a => a != -1 && !quadList[a].Processed);
                }

                if (count == 0)
                    break;

                int minCandCountIdx = -1;
                int minCandCount    = int.MaxValue;
                for (int i = 0; i < quadList.Length; i++)
                {
                    if (quadList[i].Processed)
                        continue;
                    if (quadList[i].NextCandidateCount < minCandCount)
                    {
                        minCandCount    = quadList[i].NextCandidateCount;
                        minCandCountIdx = i;
                        if (minCandCount <= 1)
                            break;
                    }
                }

                int maxQuads     = 0;
                int maxQuadsVtx0 = -1;
                int maxQuadsVtx1 = -1;
                for (int i = 0; i < 4; i++)
                {
                    int vtx0      = i;
                    int vtx1      = i == 3 ? 0 : i + 1;
                    int quadCount = TryStripInDirection(quadList, minCandCountIdx, vtx0, vtx1);
                    if (quadCount > maxQuads)
                    {
                        maxQuads     = quadCount;
                        maxQuadsVtx0 = vtx0;
                        maxQuadsVtx1 = vtx1;
                    }
                }

                if (maxQuads <= 1)
                {
                    var quad = quadList[minCandCountIdx];
                    quad.Processed = true;
                    result.Add(new Primitive(quad));
                }
                else
                    result.Add(MakeQStripPrimitive(quadList, minCandCountIdx, maxQuadsVtx0, maxQuadsVtx1));
            }

            result.AddRange(primitives.Where(a => a.Type != Primitive.PrimitiveType.Quads));
            return result.ToArray();
        }
    }
}