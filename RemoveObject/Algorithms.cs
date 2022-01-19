using System.Collections.Generic;
using System.Linq;

namespace RemoveObject
{
    public static class Algorithms
    {
        public static List<Concept> CloseByOne(Context context)
        {
            var closeByOne = new List<Concept>();
            var objects = new List<string>();
            foreach (var g in context.G)
            {
                objects.Clear();
                objects.Add(g);
                Generate(context, ref closeByOne, objects, g,
                    new Concept(context.GaluaOperatorFromObjectToSign(context.GaluaOperatorUp(objects)),
                        context.GaluaOperatorUp(objects)));
            }

            return closeByOne;
        }

        private static void Generate(Context context, ref List<Concept> closeByOne, List<string> A, string g,
            Concept concept)
        {
            var cA = new List<string>();
            cA.AddRange(concept.C);
            foreach (var a in A) cA.Remove(a);
            var any = cA.Any(h => context.G.IndexOf(g) < context.G.IndexOf(h));
            if (any == false)
            {
                var f = closeByOne.All(c => !c.C.SequenceEqual(concept.C) || !c.D.SequenceEqual(concept.D));
                if (f && concept.C.Count > 0 && concept.D.Count > 0) closeByOne.Add(concept);
            }

            var gA = new List<string>();
            gA.AddRange(context.G);
            foreach (var a in A) gA.Remove(a);
            for (var i = 0; i < gA.Count; i++)
            {
                if (context.G.IndexOf(gA[i]) >= context.G.IndexOf(g)) continue;
                gA.Remove(gA[i]);
                i--;
            }

            foreach (var f in gA)
            {
                var z = new List<string>();
                z.AddRange(concept.C);
                z.Add(f);
                var F = new List<string> {f};
                var y = concept.D.Intersect(context.GaluaOperatorUp(F)).ToList();
                var x = context.GaluaOperatorFromObjectToSign(y);

                Generate(context, ref closeByOne, z, f, new Concept(x, y));
            }
        }

        public static Grid RemoveObject(Grid grid, string removingObject)
        {
            var oldGrid = grid.Copy();
            foreach (var verticle in oldGrid.Verties
                         .Select(t => grid.Verties.Where(it => it.VerticesEquals(t)).ToList()[0])
                         .Where(verticle => verticle.C.Contains(removingObject)))
            {
                verticle.C.Remove(removingObject);
                var childrenVertices = grid.Neighbours[verticle].Where(it => it.ObjectEquals(verticle)).ToList();
                if (childrenVertices.Count > 0)
                {
                    ReplaceConceptAndEdges(ref verticle, ref grid);
                }
            }

            return grid;
        }

        private static void ReplaceConceptAndEdges(ref Concept verticle, ref Grid grid)
        {
            var tempVerticle = verticle;
            var parents = grid.Neighbours[verticle].Where(it => it.C.Count > tempVerticle.C.Count).ToList();
            var children = grid.Neighbours[verticle].Where(it => it.C.Count <= tempVerticle.C.Count).ToList();

            foreach (var par in parents)
            {
                foreach (var child in children)
                {
                    var targetChildParents = grid.Neighbours[child]
                        .Where(it => it.C.Count > child.C.Count && par.ContainObjects(it.C)).ToList();
                    if (targetChildParents.Count == 0)
                    {
                        grid.AddEdge(child, par);
                    }
                }
            }

            grid.RemoveVertex(verticle);
        }
    }
}