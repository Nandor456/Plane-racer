using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;

namespace Szeminarium1
{
    internal static class Program
    {
        private static IWindow graphicWindow;

        private static GL Gl;

        private static uint program;

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;

		out vec4 outCol;
        
        void main()
        {
			outCol = vCol;
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
        ";


        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
		
		in vec4 outCol;

        void main()
        {
            FragColor = outCol;
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "1. szeminárium - háromszög";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Load()
        {
            // egszeri beallitasokat
            //Console.WriteLine("Loaded");

            Gl = graphicWindow.CreateOpenGL();

            Gl.ClearColor(System.Drawing.Color.White);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);

            program = Gl.CreateProgram();
            Gl.AttachShader(program, vshader);
            Gl.AttachShader(program, fshader);
            Gl.LinkProgram(program);
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);

            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }

        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            // NO GL
            // make it threadsave
            //Console.WriteLine($"Update after {deltaTime} [s]");
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s]");

            Gl.Clear(ClearBufferMask.ColorBufferBit);

            uint vao = Gl.GenVertexArray();
            Gl.BindVertexArray(vao);

            float[] vertexArray = new float[] {
                //felso negyzet
                 -0.6f, 0.0f, 0.0f,  //0
                 -0.4f, -0.1f, 0.0f, //1
                 -0.2f, 0.0f, 0.0f,  //2
                 -0.4f, 0.1f, 0.0f,  //3

                 -0.4f, -0.1f, 0.0f, //4
                -0.2f, -0.2f, 0.0f,  //5
                0.0f, -0.1f, 0.0f,   //6
                -0.2f, 0.0f, 0.0f,   //7

                 -0.2f, -0.2f, 0.0f, //8
                 0.0f, -0.3f, 0.0f,  //9
                 0.2f, -0.2f, 0.0f,  //10
                 0.0f, -0.1f, 0.0f,  //11
 
                 -0.4f, 0.1f, 0.0f,   //15
                 -0.2f, 0.0f, 0.0f,  //12
                 0.0f, 0.1f, 0.0f,   //13
                 -0.2f, 0.2f, 0.0f,  //14

                -0.2f, 0.2f, 0.0f,   //16 
                 0.0f, 0.1f, 0.0f,   //17     
                 0.2f, 0.2f, 0.0f,   //18
                 0.0f, 0.3f, 0.0f,   //19

                 0.0f, -0.1f, 0.0f,  //20
                 0.2f, -0.2f, 0.0f,  //21
                 0.4f, -0.1f, 0.0f,  //22
                 0.2f, 0.0f, 0.0f,   //23

                 0.2f, 0.0f, 0.0f,   //24
                 0.4f, -0.1f, 0.0f,  //25
                 0.6f, 0.0f, 0.0f,   //26
                 0.4f, 0.1f, 0.0f,   //27

                 0.0f, 0.1f, 0.0f,  //28
                 0.2f, 0.0f, 0.0f,  //29
                 0.4f, 0.1f, 0.0f,  //30
                 0.2f, 0.2f, 0.0f, //31

                 -0.2f, 0.0f, 0.0f, //32
                 0.0f, -0.1f, 0.0f, //33
                 0.2f, 0.0f, 0.0f,  //34
                 0.0f, 0.1f, 0.0f, //35

                 //-------------------------//

                 0.0f, -0.3f, 0.0f, //36
                 0.0f, -0.53f, 0.0f,
                 0.19f, -0.44f, 0.0f,
                 0.2f, -0.2f, 0.0f,

                 0.0f, -0.53f, 0.0f, //40
                 0.0f, -0.76f, 0.0f,
                 0.18f, -0.69f, 0.0f,
                 0.19f, -0.44f, 0.0f,

                 0.0f, -0.76f, 0.0f, //44
                 0.0f, -1f, 0.0f,
                 0.17f, -0.93f, 0.0f,
                 0.18f, -0.69f, 0.0f,

                 0.2f, -0.2f, 0.0f, //48
                 0.19f, -0.44f, 0.0f,
                 0.38f, -0.35f, 0.0f,
                 0.4f, -0.1f, 0.0f,

                 0.19f, -0.44f, 0.0f,//52
                 0.18f, -0.69f, 0.0f,
                 0.36f, -0.61f, 0.0f,
                 0.38f, -0.35f, 0.0f,

                 0.18f, -0.69f, 0.0f, //56
                 0.17f, -0.93f, 0.0f,
                 0.34f, -0.86f, 0.0f,
                 0.36f, -0.61f, 0.0f,

                 0.4f, -0.1f, 0.0f, //60
                 0.38f, -0.35f, 0.0f,
                 0.57f, -0.27f, 0.0f,
                 0.6f, 0.0f, 0.0f,

                 0.38f, -0.35f, 0.0f, //64
                 0.36f, -0.61f, 0.0f,
                 0.53f, -0.54f, 0.0f,
                 0.57f, -0.27f, 0.0f,

                 0.36f, -0.61f, 0.0f, //68
                 0.34f, -0.86f, 0.0f,
                 0.5f, -0.8f, 0.0f,
                 0.53f, -0.54f, 0.0f,

                 //----------------------//

                 0.0f, -0.3f, 0.0f, //72
                 0.0f, -0.53f, 0.0f,
                 -0.19f, -0.44f, 0.0f,
                 -0.2f, -0.2f, 0.0f,

                 0.0f, -0.53f, 0.0f, //76
                 0.0f, -0.76f, 0.0f,
                 -0.18f, -0.69f, 0.0f,
                 -0.19f, -0.44f, 0.0f,

                 0.0f, -0.76f, 0.0f, //80
                 0.0f, -1f, 0.0f,
                 -0.17f, -0.93f, 0.0f,
                 -0.18f, -0.69f, 0.0f,

                 -0.2f, -0.2f, 0.0f, //84
                 -0.19f, -0.44f, 0.0f,
                 -0.38f, -0.35f, 0.0f,
                 -0.4f, -0.1f, 0.0f,

                 -0.19f, -0.44f, 0.0f,//88
                 -0.18f, -0.69f, 0.0f,
                 -0.36f, -0.61f, 0.0f,
                 -0.38f, -0.35f, 0.0f,

                 -0.18f, -0.69f, 0.0f, //92
                 -0.17f, -0.93f, 0.0f,
                 -0.34f, -0.86f, 0.0f,
                 -0.36f, -0.61f, 0.0f,

                 -0.4f, -0.1f, 0.0f, //96
                 -0.38f, -0.35f, 0.0f,
                 -0.57f, -0.27f, 0.0f,
                 -0.6f, 0.0f, 0.0f,

                 -0.38f, -0.35f, 0.0f, //100
                 -0.36f, -0.61f, 0.0f,
                 -0.53f, -0.54f, 0.0f,
                 -0.57f, -0.27f, 0.0f,

                 -0.36f, -0.61f, 0.0f, //104
                 -0.34f, -0.86f, 0.0f,
                 -0.5f, -0.8f, 0.0f,
                 -0.53f, -0.54f, 0.0f
            };

            float[] colorArray = new float[] {
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                //-----------------------//

                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,

                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,

                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,

                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                //----------------------//

                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,

                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,
                0.0f, 1.0f, 1.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,

                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,

                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,
                1.0f, 0.0f, 1.0f, 1.0f,

                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,
                1.0f, 1.0f, 0.0f, 1.0f,

                
            };

            uint[] indexArray = new uint[] {
                0, 1, 3,
                1, 2, 3,
                4, 5, 7,
                5, 6, 7,
                8, 9, 11,
                9, 10, 11,
                12, 13, 15,
                13, 14, 15,
                16, 17, 19,
                17, 18, 19,
                20, 21, 23,
                21, 22, 23,
                24, 25, 27,
                25, 26, 27,
                28, 29, 31,
                29, 30, 31,
                32, 33, 35,
                33, 34, 35,
                36, 37, 39,
                37, 38, 39,
                40, 41, 43,
                41, 42, 43,
                44, 45, 47,
                45, 46, 47,
                48, 49, 51,
                49, 50, 51,
                52, 53, 55,
                53, 54, 55,
                56, 57, 59,
                57, 58, 59,
                60, 61, 63,
                61, 62, 63,
                64, 65, 67,
                65, 66, 67,
                68, 69, 71,
                69, 70, 71, 
                72, 73, 75,
                73, 74, 75,
                76, 77, 79,
                77, 78, 79,
                80, 81, 83,
                81, 82, 83,
                84, 85, 87,
                85, 86, 87,
                88, 89, 91,
                89, 90, 91,
                92, 93, 95,
                93, 94, 95,
                96, 97, 99,
                97, 98, 99,
                100, 101, 103,
                101, 102, 103,
                104, 105, 107,
                105, 106, 107
            };

            uint vertices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, vertices);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)vertexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(0);

            uint colors = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, colors);
            Gl.BufferData(GLEnum.ArrayBuffer, (ReadOnlySpan<float>)colorArray.AsSpan(), GLEnum.StaticDraw);
            Gl.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, null);
            Gl.EnableVertexAttribArray(1);

            uint indices = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, indices);
            Gl.BufferData(GLEnum.ElementArrayBuffer, (ReadOnlySpan<uint>)indexArray.AsSpan(), GLEnum.StaticDraw);
            Gl.BindBuffer(GLEnum.ArrayBuffer, 0);
            Gl.UseProgram(program);

            Gl.DrawElements(GLEnum.Triangles, (uint)indexArray.Length, GLEnum.UnsignedInt, null); // we used element buffer
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(vao);

            // always unbound the vertex buffer first, so no halfway results are displayed by accident
            Gl.DeleteBuffer(vertices);
            Gl.DeleteBuffer(colors);
            Gl.DeleteBuffer(indices);
            Gl.DeleteVertexArray(vao);
        }
    }
}
