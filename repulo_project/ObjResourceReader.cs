using repulo_project;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using System.Globalization;

namespace repulo_project
{
    internal class ObjResourceReader
    {
        private struct FaceVertex
        {
            public int VertexIndex;
            public int? NormalIndex;

            public FaceVertex(int vertexIndex, int? normalIndex)
            {
                VertexIndex = vertexIndex;
                NormalIndex = normalIndex;
            }
        }

        public static unsafe GlObject CreateTeapotWithColor(GL Gl, float[] faceColor)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            List<float[]> objVertices;
            List<float[]> objNormals;
            List<List<FaceVertex>> objFaces;

            ReadObjData(out objVertices, out objNormals, out objFaces);

            List<float> glVertices = new List<float>();
            List<float> glColors = new List<float>();
            List<uint> glIndices = new List<uint>();

            CreateGlArraysFromObjArrays(faceColor, objVertices, objNormals, objFaces, glVertices, glColors, glIndices);

            return CreateOpenGlObject(Gl, vao, glVertices, glColors, glIndices);
        }

        private static unsafe GlObject CreateOpenGlObject(GL Gl, uint vao, List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            uint offsetPos = 0;
            uint offsetNormal = offsetPos + (3 * sizeof(float));
            uint vertexSize = offsetNormal + (3 * sizeof(float));

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glVertices.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetPos);
            Gl.EnableVertexAttribArray(0);

            Gl.EnableVertexAttribArray(2);
            Gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexSize, (void*)offsetNormal);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)glColors.ToArray().AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)glIndices.ToArray().AsSpan(), GLEnum.StaticDraw);

            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            uint indexArrayLength = (uint)glIndices.Count;

            return new GlObject(vao, vertices, colors, indices, indexArrayLength, Gl);
        }

        private static unsafe void CreateGlArraysFromObjArrays(float[] faceColor,
            List<float[]> objVertices, List<float[]> objNormals, List<List<FaceVertex>> objFaces,
            List<float> glVertices, List<float> glColors, List<uint> glIndices)
        {
            Dictionary<string, int> glVertexIndices = new Dictionary<string, int>();

            foreach (var face in objFaces)
            {
                Vector3D<float> normal;

                bool hasNormal = face.All(fv => fv.NormalIndex.HasValue);
                if (!hasNormal)
                {
                    var aArray = objVertices[face[0].VertexIndex - 1];
                    var a = new Vector3D<float>(aArray[0], aArray[1], aArray[2]);
                    var bArray = objVertices[face[1].VertexIndex - 1];
                    var b = new Vector3D<float>(bArray[0], bArray[1], bArray[2]);
                    var cArray = objVertices[face[2].VertexIndex - 1];
                    var c = new Vector3D<float>(cArray[0], cArray[1], cArray[2]);

                    normal = Vector3D.Normalize(Vector3D.Cross(b - a, c - a));
                }
                else
                {
                    var n = objNormals[face[0].NormalIndex!.Value - 1];
                    normal = new Vector3D<float>(n[0], n[1], n[2]);
                }

                foreach (var fv in face)
                {
                    var vertex = objVertices[fv.VertexIndex - 1];
                    List<float> glVertex = new List<float>(vertex);

                    if (fv.NormalIndex.HasValue)
                    {
                        var n = objNormals[fv.NormalIndex.Value - 1];
                        glVertex.AddRange(n);
                    }
                    else
                    {
                        glVertex.Add(normal.X);
                        glVertex.Add(normal.Y);
                        glVertex.Add(normal.Z);
                    }

                    string key = string.Join(",", glVertex);
                    if (!glVertexIndices.ContainsKey(key))
                    {
                        glVertices.AddRange(glVertex);
                        glColors.AddRange(faceColor);
                        glVertexIndices[key] = glVertexIndices.Count;
                    }

                    glIndices.Add((uint)glVertexIndices[key]);
                }
            }
        }

        private static unsafe void ReadObjData(out List<float[]> objVertices,
            out List<float[]> objNormals, out List<List<FaceVertex>> objFaces)
        {
            objVertices = new List<float[]>();
            objNormals = new List<float[]>();
            objFaces = new List<List<FaceVertex>>();

            using (Stream objStream = typeof(ObjResourceReader).Assembly
                .GetManifestResourceStream("repulo_project.Resources.plane.obj"))
            {
                if (objStream == null)
                {
                    throw new FileNotFoundException(
                        "Could not find embedded resource 'repulo_project.Resources.plane.obj");
                }

                using (StreamReader objReader = new StreamReader(objStream))
                {
                    while (!objReader.EndOfStream)
                    {
                        var line = objReader.ReadLine();

                        if (string.IsNullOrWhiteSpace(line) || line.Trim().StartsWith("#"))
                            continue;

                        var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length == 0) continue;

                        switch (parts[0])
                        {
                            case "v":
                                Console.WriteLine("ez v");
                                objVertices.Add(parts.Skip(1).Take(3)
                                    .Select(p => float.Parse(p, CultureInfo.InvariantCulture))
                                    .ToArray());
                                break;

                            case "vn":
                                Console.WriteLine("ez vn");
                                objNormals.Add(parts.Skip(1).Take(3)
                                    .Select(p => float.Parse(p, CultureInfo.InvariantCulture))
                                    .ToArray());
                                break;

                            case "f":
                                Console.WriteLine("ez f");
                                List<FaceVertex> face = new List<FaceVertex>();
                                foreach (var part in parts.Skip(1))
                                {
                                    var tokens = part.Split('/');
                                    int v = int.Parse(tokens[0]);
                                    int? vn = tokens.Length >= 3 && !string.IsNullOrEmpty(tokens[2])
                                        ? int.Parse(tokens[2])
                                        : null;
                                    face.Add(new FaceVertex(v, vn));
                                }
                                objFaces.Add(face);
                                break;
                        }
                    }
                }
            }
        }
    }
}