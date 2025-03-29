using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;
using Szeminarium;

namespace GrafikaSzeminarium
{
    internal class Program
    {
        private static IWindow graphicWindow;
        private static GL Gl;
        private static List<ModelObjectDescriptor> cubes = new List<ModelObjectDescriptor>();
        private static CameraDescriptor camera = new CameraDescriptor();
        private static CubeArrangementModel cubeArrangementModel = new CubeArrangementModel();

        private const string ModelMatrixVariableName = "uModel";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec4 vCol;

        uniform mat4 uModel;
        uniform mat4 uView;
        uniform mat4 uProjection;
            
        out vec4 fCol;
        
        void main()
        {
            gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
            fCol = vCol;
        }
        ";

        private static readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        in vec4 fCol;
        
        void main()
        {
            FragColor = fCol;
        }
        ";

        private static uint program;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "Grafika szeminárium";
            windowOptions.Size = new Silk.NET.Maths.Vector2D<int>(500, 500);

            graphicWindow = Window.Create(windowOptions);

            graphicWindow.Load += GraphicWindow_Load;
            graphicWindow.Update += GraphicWindow_Update;
            graphicWindow.Render += GraphicWindow_Render;
            graphicWindow.Closing += GraphicWindow_Closing;

            graphicWindow.Run();
        }

        private static void GraphicWindow_Closing()
        {
            foreach (var cube in cubes)
            {
                cube.Dispose();
            }
            Gl.DeleteProgram(program);
        }

        private static void GraphicWindow_Load()
        {
            Gl = graphicWindow.CreateOpenGL();

            var inputContext = graphicWindow.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            int index = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        float[] colorArray = GenerateRubikColorArray(x, y, z);
                        cubes.Add(ModelObjectDescriptor.CreateCube(Gl, colorArray));
                        index++;
                    }
                }
            }

            Gl.ClearColor(System.Drawing.Color.White);
            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(TriangleFace.Back);
            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);

            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, VertexShaderSource);
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, FragmentShaderSource);
            Gl.CompileShader(fshader);
            Gl.GetShader(fshader, ShaderParameterName.CompileStatus, out int fStatus);
            if (fStatus != (int)GLEnum.True)
                throw new Exception("Fragment shader failed to compile: " + Gl.GetShaderInfoLog(fshader));

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

        private static float[] GenerateRubikColorArray(int x, int y, int z)
        {
            float[] colorArray = new float[24 * 4]; // 24 vertices, 4 components each (RGBA)

            // Standard Rubik's Cube colors
            Vector4 white = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 yellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            Vector4 red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            Vector4 orange = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
            Vector4 blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            Vector4 green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            Vector4 black = new Vector4(0.1f, 0.1f, 0.1f, 1.0f);

            // Face order matches your vertex array:
            // 0-3: Top face
            // 4-7: Front face
            // 8-11: Left face
            // 12-15: Bottom face
            // 16-19: Back face
            // 20-23: Right face

            // Top face (white) - only color if this is the top layer
            Vector4 topColor = y == 1 ? white : black;
            for (int i = 0; i < 4 * 4; i += 4)
            {
                colorArray[i] = topColor.X;
                colorArray[i + 1] = topColor.Y;
                colorArray[i + 2] = topColor.Z;
                colorArray[i + 3] = topColor.W;
            }

            // Front face (red) - only color if this is the front layer
            Vector4 frontColor = z == 1 ? red : black;
            for (int i = 4 * 4; i < 8 * 4; i += 4)
            {
                colorArray[i] = frontColor.X;
                colorArray[i + 1] = frontColor.Y;
                colorArray[i + 2] = frontColor.Z;
                colorArray[i + 3] = frontColor.W;
            }

            // Left face (blue) - only color if this is the left layer
            Vector4 leftColor = x == -1 ? blue : black;
            for (int i = 8 * 4; i < 12 * 4; i += 4)
            {
                colorArray[i] = leftColor.X;
                colorArray[i + 1] = leftColor.Y;
                colorArray[i + 2] = leftColor.Z;
                colorArray[i + 3] = leftColor.W;
            }

            // Bottom face (yellow) - only color if this is the bottom layer
            Vector4 bottomColor = y == -1 ? yellow : black;
            for (int i = 12 * 4; i < 16 * 4; i += 4)
            {
                colorArray[i] = bottomColor.X;
                colorArray[i + 1] = bottomColor.Y;
                colorArray[i + 2] = bottomColor.Z;
                colorArray[i + 3] = bottomColor.W;
            }

            // Back face (orange) - only color if this is the back layer
            Vector4 backColor = z == -1 ? orange : black;
            for (int i = 16 * 4; i < 20 * 4; i += 4)
            {
                colorArray[i] = backColor.X;
                colorArray[i + 1] = backColor.Y;
                colorArray[i + 2] = backColor.Z;
                colorArray[i + 3] = backColor.W;
            }

            // Right face (green) - only color if this is the right layer
            Vector4 rightColor = x == 1 ? green : black;
            for (int i = 20 * 4; i < 24 * 4; i += 4)
            {
                colorArray[i] = rightColor.X;
                colorArray[i + 1] = rightColor.Y;
                colorArray[i + 2] = rightColor.Z;
                colorArray[i + 3] = rightColor.W;
            }

            return colorArray;
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    camera.DecreaseZYAngle();
                    break;
                case Key.Right:
                    camera.IncreaseZYAngle();
                    break;
                case Key.Down:
                    camera.IncreaseDistance();
                    break;
                case Key.Up:
                    camera.DecreaseDistance();
                    break;
                case Key.U:
                    camera.IncreaseZXAngle();
                    break;
                case Key.D:
                    camera.DecreaseZXAngle();
                    break;
                case Key.Space:
                    cubeArrangementModel.AnimationEnabled = !cubeArrangementModel.AnimationEnabled;
                    break;
            }
        }

        private static void GraphicWindow_Update(double deltaTime)
        {
            cubeArrangementModel.AdvanceTime(deltaTime);
        }

        private static unsafe void GraphicWindow_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(program);

            var viewMatrix = Matrix4X4.CreateLookAt(camera.Position, camera.Target, camera.UpVector);
            SetMatrix(viewMatrix, ViewMatrixVariableName);

            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)(Math.PI / 2), 1024f / 768f, 0.1f, 100f);
            SetMatrix(projectionMatrix, ProjectionMatrixVariableName);

            float spacing = 1.1f;
            int cubeIndex = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        var translation = Matrix4X4.CreateTranslation(x * spacing, y * spacing, z * spacing);
                        SetMatrix(translation, ModelMatrixVariableName);
                        DrawModelObject(cubes[cubeIndex]);
                        cubeIndex++;
                    }
                }
            }
        }

        private static unsafe void DrawModelObject(ModelObjectDescriptor modelObject)
        {
            Gl.BindVertexArray(modelObject.Vao);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, modelObject.Indices);
            Gl.DrawElements(PrimitiveType.Triangles, modelObject.IndexArrayLength, DrawElementsType.UnsignedInt, null);
            Gl.BindBuffer(GLEnum.ElementArrayBuffer, 0);
            Gl.BindVertexArray(0);
        }

        private static unsafe void SetMatrix(Matrix4X4<float> mx, string uniformName)
        {
            int location = Gl.GetUniformLocation(program, uniformName);
            if (location == -1)
            {
                throw new Exception($"{uniformName} uniform not found on shader.");
            }
            Gl.UniformMatrix4(location, 1, false, (float*)&mx);
        }
    }
}