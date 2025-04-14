using GrafikaSzeminarium;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Numerics;

namespace Szeminarium1_24_02_17_2
{
    internal static class Program
    {
        private static CameraDescriptor cameraDescriptor = new();

        private static CubeArrangementModel cubeArrangementModel = new();

        private static IWindow window;

        private static IInputContext inputContext;

        private static GL Gl;

        private static ImGuiController controller;

        private static uint program;

        private static List<ModelObjectDescriptor> rubiksCubes = new List<ModelObjectDescriptor>();


        private static float Shininess = 50;
        private static Vector3 AmbientStrength = new Vector3(0.5f);
        private static Vector3 DiffuseStrength = new Vector3(0.3f);
        private static Vector3 SpecularStrength = new Vector3(0.5f);
        private static Vector3 LightColor = Vector3.One;
        private static int SelectedFace = 0;
        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private static readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
		layout (location = 1) in vec4 vCol;
        layout (location = 2) in vec3 vNorm;

        uniform mat4 uModel;
        uniform mat3 uNormal;
        uniform mat4 uView;
        uniform mat4 uProjection;

		out vec4 outCol;
        out vec3 outNormal;
        out vec3 outWorldPosition;
        
        void main()
        {
			outCol = vCol;
            gl_Position = uProjection*uView*uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0);
            outNormal = uNormal*vNorm;
            outWorldPosition = vec3(uModel*vec4(vPos.x, vPos.y, vPos.z, 1.0));
        }
        ";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";

        private static readonly string FragmentShaderSource = @"
        #version 330 core
        
        uniform vec3 lightColor;
        uniform vec3 lightPos;
        uniform vec3 viewPos;
        uniform float shininess;
        uniform vec3 ambientStrength;
        uniform vec3 diffuseStrength;
        uniform vec3 specularStrength;
        out vec4 FragColor;

		in vec4 outCol;
        in vec3 outNormal;
        in vec3 outWorldPosition;

        void main()
        {
            vec3 ambient = ambientStrength * lightColor;

            vec3 norm = normalize(outNormal);
            vec3 lightDir = normalize(lightPos - outWorldPosition);
            float diff = max(dot(norm, lightDir), 0.0);
            vec3 diffuse = diff * lightColor * diffuseStrength;

            vec3 viewDir = normalize(viewPos - outWorldPosition);
            vec3 reflectDir = reflect(-lightDir, norm);
            float spec = pow(max(dot(viewDir, reflectDir), 0.0), shininess);
            vec3 specular = specularStrength * spec * lightColor;
  

            vec3 result = (ambient + diffuse + specular) * outCol.xyz;
            FragColor = vec4(result, outCol.w);
        }
        ";

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "2 szeminárium";
            windowOptions.Size = new Vector2D<int>(500, 500);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }

        private static void Window_Load()
        {
            //Console.WriteLine("Load");

            // set up input handling
            inputContext = window.CreateInput();
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            Gl = window.CreateOpenGL();

            controller = new ImGuiController(Gl, window, inputContext);

            // Handle resizes
            window.FramebufferResize += s =>
            {
                // Adjust the viewport to the new window size
                Gl.Viewport(s);
            };


            Gl.ClearColor(System.Drawing.Color.White);

            SetUpObjects();

            LinkProgram();

            Gl.Enable(EnableCap.CullFace);

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
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
            Gl.GetProgram(program, GLEnum.LinkStatus, out var status);
            if (status == 0)
            {
                Console.WriteLine($"Error linking shader {Gl.GetProgramInfoLog(program)}");
            }
            Gl.DetachShader(program, vshader);
            Gl.DetachShader(program, fshader);
            Gl.DeleteShader(vshader);
            Gl.DeleteShader(fshader);
        }

        private static void Keyboard_KeyDown(IKeyboard keyboard, Key key, int arg3)
        {
            switch (key)
            {
                case Key.Left:
                    cameraDescriptor.DecreaseZYAngle();
                    break;
                    ;
                case Key.Right:
                    cameraDescriptor.IncreaseZYAngle();
                    break;
                case Key.Down:
                    cameraDescriptor.IncreaseDistance();
                    break;
                case Key.Up:
                    cameraDescriptor.DecreaseDistance();
                    break;
                case Key.U:
                    cameraDescriptor.IncreaseZXAngle();
                    break;
                case Key.D:
                    cameraDescriptor.DecreaseZXAngle();
                    break;
                case Key.Space:
                    cubeArrangementModel.AnimationEnabeld = !cubeArrangementModel.AnimationEnabeld;
                    break;
            }
        }

        private static void Window_Update(double deltaTime)
        {
            //Console.WriteLine($"Update after {deltaTime} [s].");
            // multithreaded
            // make sure it is threadsafe
            // NO GL calls
            cubeArrangementModel.AdvanceTime(deltaTime);

            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();
            SetAmbientStrength();
            SetDiffuseStrength();
            SetSpecularStrength();

            DrawRubiksCube();

            RenderUI();

            controller.Render();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 3f, 3f, 3f);
            CheckError();
        }

        private static unsafe void SetViewerPosition()
        {
            int location = Gl.GetUniformLocation(program, ViewPosVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewPosVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
            CheckError();
        }

        private static unsafe void SetShininess()
        {
            int location = Gl.GetUniformLocation(program, ShininessVariableName);

            if (location == -1)
            {
                throw new Exception($"{ShininessVariableName} uniform not found on shader.");
            }
            Gl.Uniform1(location, Shininess);
            CheckError();
        }

        private static unsafe void SetModelMatrix(Matrix4X4<float> modelMatrix)
        {
            int location = Gl.GetUniformLocation(program, ModelMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{ModelMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&modelMatrix);
            CheckError();

            var modelMatrixWithoutTranslation = new Matrix4X4<float>(modelMatrix.Row1, modelMatrix.Row2, modelMatrix.Row3, modelMatrix.Row4);
            modelMatrixWithoutTranslation.M41 = 0;
            modelMatrixWithoutTranslation.M42 = 0;
            modelMatrixWithoutTranslation.M43 = 0;
            modelMatrixWithoutTranslation.M44 = 1;

            Matrix4X4<float> modelInvers;
            Matrix4X4.Invert<float>(modelMatrixWithoutTranslation, out modelInvers);
            Matrix3X3<float> normalMatrix = new Matrix3X3<float>(Matrix4X4.Transpose(modelInvers));
            location = Gl.GetUniformLocation(program, NormalMatrixVariableName);
            if (location == -1)
            {
                throw new Exception($"{NormalMatrixVariableName} uniform not found on shader.");
            }
            Gl.UniformMatrix3(location, 1, false, (float*)&normalMatrix);
            CheckError();
        }

        private static unsafe void SetUpObjects()
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        float[] colorArray = GenerateRubikColorArray(x, y, z);
                        rubiksCubes.Add(ModelObjectDescriptor.CreateCube(Gl, colorArray));
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
        private static unsafe void DrawRubiksCube()
        {
            float spacing = 1.1f;
            int cubeIndex = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Matrix4X4<float> modelMatrix = Matrix4X4.CreateTranslation(x * spacing, y * spacing, z * spacing);
                        SetModelMatrix(modelMatrix);
                        DrawModelObject(rubiksCubes[cubeIndex]);
                        cubeIndex++;
                    }
                }
            }
        }
        private static void Window_Closing()
        {
            foreach (var cube in rubiksCubes)
            {
                cube.Dispose();
            }
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 100);
            int location = Gl.GetUniformLocation(program, ProjectionMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&projectionMatrix);
            CheckError();
        }

        private static unsafe void SetViewMatrix()
        {
            var viewMatrix = Matrix4X4.CreateLookAt(cameraDescriptor.Position, cameraDescriptor.Target, cameraDescriptor.UpVector);
            int location = Gl.GetUniformLocation(program, ViewMatrixVariableName);

            if (location == -1)
            {
                throw new Exception($"{ViewMatrixVariableName} uniform not found on shader.");
            }

            Gl.UniformMatrix4(location, 1, false, (float*)&viewMatrix);
            CheckError();
        }

        public static void CheckError()
        {
            var error = (ErrorCode)Gl.GetError();
            if (error != ErrorCode.NoError)
                throw new Exception("GL.GetError() returned " + error.ToString());
        }
        private static unsafe void SetAmbientStrength()
        {
            int location = Gl.GetUniformLocation(program, "ambientStrength");
            if (location == -1) throw new Exception("ambientStrength uniform not found");
            Gl.Uniform3(location, AmbientStrength.X, AmbientStrength.Y, AmbientStrength.Z);
            CheckError();
        }

        private static unsafe void SetDiffuseStrength()
        {
            int location = Gl.GetUniformLocation(program, "diffuseStrength");
            if (location == -1) throw new Exception("diffuseStrength uniform not found");
            Gl.Uniform3(location, DiffuseStrength.X, DiffuseStrength.Y, DiffuseStrength.Z);
            CheckError();
        }

        private static unsafe void SetSpecularStrength()
        {
            int location = Gl.GetUniformLocation(program, "specularStrength");
            if (location == -1) throw new Exception("specularStrength uniform not found");
            Gl.Uniform3(location, SpecularStrength.X, SpecularStrength.Y, SpecularStrength.Z);
            CheckError();
        }

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);
            if (location == -1) throw new Exception($"{LightColorVariableName} uniform not found");
            Gl.Uniform3(location, LightColor.X, LightColor.Y, LightColor.Z);
            CheckError();
        }
        private static void RenderUI()
        {
            ImGui.Begin("Lighting Properties",
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);

            ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);

            ImGui.Separator();
            ImGui.Text("Light Properties");
            ImGui.ColorEdit3("Light Color", ref LightColor);

            ImGui.Separator();
            ImGui.Text("Material Properties");
            ImGui.SliderFloat3("Ambient", ref AmbientStrength, 0, 1);
            ImGui.SliderFloat3("Diffuse", ref DiffuseStrength, 0, 1);
            ImGui.SliderFloat3("Specular", ref SpecularStrength, 0, 1);

        }
        private static float[] GenerateRubikColorArray(int x, int y, int z)
        {
            float[] colorArray = new float[24 * 4]; // 24 vertices, 4 components each (RGBA)

            Vector4 white = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            Vector4 yellow = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
            Vector4 red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            Vector4 orange = new Vector4(1.0f, 0.5f, 0.0f, 1.0f);
            Vector4 blue = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
            Vector4 green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            Vector4 black = new Vector4(0.1f, 0.1f, 0.1f, 1.0f); // For non-visible faces

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

    }
}