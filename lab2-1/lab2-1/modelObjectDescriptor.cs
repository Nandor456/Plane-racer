using Silk.NET.OpenGL;
using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GrafikaSzeminarium
{
    internal class ModelObjectDescriptor : IDisposable
    {
        private bool disposedValue;

        public uint Vao { get; private set; }
        public uint Vertices { get; private set; }
        public uint Colors { get; private set; }
        public uint Indices { get; private set; }
        public uint IndexArrayLength { get; private set; }

        private GL Gl;

        public unsafe static ModelObjectDescriptor CreateCube(GL Gl)
        {
            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float s = 0.5f; // Cube size

            var vertexArray = new float[27 * 24 * 3]; // 27 cubes, 24 vertices per cube, 3 coordinates per vertex
            var colorArray = new float[27 * 24 * 4];  // 27 cubes, 24 vertices, 4 color channels (RGBA)
            var indexArray = new uint[27 * 36];       // 27 cubes, 6 faces, 2 triangles per face, 3 indices per triangle

            int vertexIndex = 0;
            int colorIndex = 0;
            int indexIndex = 0;
            uint vertexOffset = 0;

            Random rand = new Random();

            for (float x = -1; x <= 0; x += s)
            {
                for (float y = -1; y <= 0; y += s)
                {
                    for (float z = -1; z <= 0; z += s)
                    {
                        float[] cubeVertices = {
                            //bottom face
                            x, y, z,   x + s, y, z,   x + s, y + s, z,   x, y + s, z,
                            //fron face
                            x, y, z,   x + s, y, z,   x + s, y, z + s,   x, y, z + s,
                            //right face
                            x + s, y, z,   x + s, y + s, z,   x + s, y + s, z + s,   x + s, y, z + s,
                            // left face
                            x, y, z,   x, y + s, z,   x, y + s, z + s,   x, y, z + s,
                            // top face
                            x, y, z + s,   x + s, y, z + s,   x + s, y + s, z + s,   x, y + s, z + s,
                            // back face
                            x, y + s, z,   x + s, y + s, z,   x + s, y + s, z + s,   x, y + s, z + s,
                        };

                        uint[] cubeIndices = {
                            0, 1, 2,  2, 3, 0,  // Front
                            4, 5, 6,  6, 7, 4,  // Back
                            8, 9, 10, 10, 11, 8,  // Left
                            12, 13, 14, 14, 15, 12,  // Right
                            16, 17, 18, 18, 19, 16,  // Bottom
                            20, 21, 22, 22, 23, 20   // Top
                        };

                        // Random color for each cube
                        float r = (float)rand.NextDouble();
                        float g = (float)rand.NextDouble();
                        float b = (float)rand.NextDouble();
                        float a = 1.0f;

                        // Add vertices
                        for (int i = 0; i < cubeVertices.Length; i++)
                        {
                            if (i % 3 == 0) {
                                
                                Console.WriteLine(")");
                                Console.Write("(");
                            }
                            vertexArray[vertexIndex++] = cubeVertices[i];
                            Console.Write($"{cubeVertices[i]},");
                            
                        }

                        // Add indices
                        for (int i = 0; i < cubeIndices.Length; i++)
                        {
                            indexArray[indexIndex++] = cubeIndices[i] + vertexOffset;
                        }

                        // Add colors
                        for (int i = 0; i < 24; i++) // 24 vertices per cube
                        {
                            colorArray[colorIndex++] = r;
                            colorArray[colorIndex++] = g;
                            colorArray[colorIndex++] = b;
                            colorArray[colorIndex++] = a;
                        }

                        vertexOffset += 24; // Each cube has 24 vertices
                    }
                }
            }

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);

            return new ModelObjectDescriptor() { Vao = vao, Vertices = vertices, Colors = colors, Indices = indices, IndexArrayLength = (uint)indexArray.Length, Gl = Gl };

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null


                // always unbound the vertex buffer first, so no halfway results are displayed by accident
                Gl.DeleteBuffer(Vertices);
                Gl.DeleteBuffer(Colors);
                Gl.DeleteBuffer(Indices);
                Gl.DeleteVertexArray(Vao);

                disposedValue = true;
            }
        }

        ~ModelObjectDescriptor()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
