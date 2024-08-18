using CPU_Doom.Buffers;
using SharpGL.SceneGraph;
using SharpGL.Serialization.Wavefront;
using SharpGL.SceneGraph.Primitives;
using CPU_Doom.Types;


namespace CPU_Doom_File_Loader
{
    public static class ObjectLoader
    {
        public static List<VertexArrayObject> LoadVAOsFromObjFile(string objFile)
        {
            
            ObjFileFormat fileFormat = new ObjFileFormat();
            Scene scene = fileFormat.LoadData(objFile);

            List<VertexArrayObject> vaos = new List<VertexArrayObject>();
            foreach (var polygon in scene.SceneContainer.Traverse<Polygon>())
            {
                var distinctIndices =
                    (from face in polygon.Faces from index in face.Indices select index)
                    .DistinctBy(index => new IndexComperable(index));
                var distinctIndicesSorted = distinctIndices.Select(
                    (index, loc) => (new IndexComperable(index), loc) 
                    ).ToDictionary();

                byte[] vertexPositions = polygon.Vertices.SelectMany(vertex => new float[] { vertex.X, vertex.Y, vertex.Z }).ToArray().ToByteArray();
                byte[] vertexNormals = polygon.Vertices.SelectMany(normal => new float[] { normal.X, normal.Y, normal.Z }).ToArray().ToByteArray();
                byte[] vertexUV = polygon.UVs.SelectMany(uv => new float[] { uv.U, uv.V }).ToArray().ToByteArray();

                byte[][] vertexData = new byte[][]
                {
                    vertexPositions,
                    vertexNormals,
                    vertexUV,
                };

                Stride stride = new Stride();
                stride.AddEntry(PIXELTYPE.FLOAT, 3);
                stride.AddEntry(PIXELTYPE.FLOAT, 3);
                stride.AddEntry(PIXELTYPE.FLOAT, 2);


                int[,] vertices = new int[distinctIndicesSorted.Values.Count, 3];
                int vertexIndex = 0;
                foreach (var vertex in distinctIndices)
                {
                    vertices[vertexIndex, 0] = vertex.Vertex;
                    vertices[vertexIndex, 1] = vertex.Normal;
                    vertices[vertexIndex, 2] = vertex.UV;
                    ++vertexIndex;
                }
                ParallelVertexBuffer vertexBuffer = new ParallelVertexBuffer(stride, vertexData, vertices);

                polygon.Faces = Triangulate(polygon.Faces);
                int[] faceIndices = polygon.Faces.SelectMany(face => 
                from index in face.Indices select distinctIndicesSorted[new IndexComperable(index)]
                ).ToArray();

                vaos.Add(new VertexArrayObject(faceIndices, vertexBuffer));
            }
            return vaos;
        }

        private static List<Face> Triangulate(List<Face> faces)
        {
            // Note: Ignore Neighbouring Indeces...(hard to implement and not worth it)
            var result = new List<Face>();
            foreach (Face face in faces)
            {
                int numIndices = face.Count;

                for (int i = 1; i < numIndices - 1; i++)
                {
                    Face newFace = new Face()
                    {
                        Indices = new()
                        {
                            face.Indices[0],
                            face.Indices[i],
                            face.Indices[i + 1]
                        },
                        Material = face.Material
                    };
                    result.Add(newFace);
                }
            }
            return result; 
        } 

        struct IndexComperable : IComparable<IndexComperable>
        {
            public int Vertex, UV, Normal;

            public IndexComperable(SharpGL.SceneGraph.Index index)
            {
                Vertex = index.Vertex;
                UV = index.UV;
                Normal = index.Normal;
            }

            public int CompareTo(IndexComperable other)
            {
                if (Vertex != other.Vertex) return Vertex.CompareTo(other.Vertex);
                if (UV != other.UV) return UV.CompareTo(other.UV);
                return Normal.CompareTo(other.Normal);
            }
        }


    }
}
