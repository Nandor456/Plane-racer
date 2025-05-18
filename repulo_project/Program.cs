using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using System.Text;

namespace repulo_project
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

        private static GlObject plane;

        private static GlObject table;

        private static GlCube skyBox;

        private static GlObject glSphere;

        private static GlObject glPlate;

        private static List<Ring> rings = new();

        private static Vector3D<float> planePosition = new(0f, 10f, 0f);
        private static float planeYaw = 0f;
        private static float planeSpeed = 30f;

        private static float Shininess = 50;

        private static bool DrawWireFrameOnly = false;

        private const string ModelMatrixVariableName = "uModel";
        private const string NormalMatrixVariableName = "uNormal";
        private const string ViewMatrixVariableName = "uView";
        private const string ProjectionMatrixVariableName = "uProjection";

        private const string TextureUniformVariableName = "uTexture";

        private const string LightColorVariableName = "lightColor";
        private const string LightPositionVariableName = "lightPos";
        private const string ViewPosVariableName = "viewPos";
        private const string ShininessVariableName = "shininess";
        private static float planeRoll = 0f;
        private static float rollTarget = 0f;
        private static float maxRoll = MathF.PI / 18f;
        private static float rollSpeed = 5f;
        private static bool cameraInFront = false;
        private static GlObject ringTorus;
        private static float planePitch = 0f;

        static void Main(string[] args)
        {
            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = "2 szeminárium";
            windowOptions.Size = new Vector2D<int>(1000, 1000);

            // on some systems there is no depth buffer by default, so we need to make sure one is created
            windowOptions.PreferredDepthBufferBits = 24;

            window = Window.Create(windowOptions);

            window.Load += Window_Load;
            window.Update += Window_Update;
            window.Render += Window_Render;
            window.Closing += Window_Closing;

            window.Run();
        }
        private static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private static float Clamp(float value, float min, float max)
        {
            return MathF.Min(MathF.Max(value, min), max);
        }
        private static void Window_Load()
        {
            inputContext = window.CreateInput();
            foreach (var ring in rings)
            {
                float distance = Vector3D.Distance(planePosition, ring.Position);
                if (distance < ring.Radius)
                {
                    Console.WriteLine("Passed through ring!");
                    // Optionally remove or mark it
                }
            }
            Gl = window.CreateOpenGL();
            controller = new ImGuiController(Gl, window, inputContext);
            foreach (var keyboard in inputContext.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }
            window.FramebufferResize += s =>
            {
                Gl.Viewport(s);
            };

            Gl.ClearColor(System.Drawing.Color.White);

            SetUpObjects();
            LinkProgram();

            Gl.Enable(EnableCap.DepthTest);
            Gl.DepthFunc(DepthFunction.Lequal);
        }

        private static void LinkProgram()
        {
            uint vshader = Gl.CreateShader(ShaderType.VertexShader);
            uint fshader = Gl.CreateShader(ShaderType.FragmentShader);

            Gl.ShaderSource(vshader, ReadShader("VertexShader.vert"));
            Gl.CompileShader(vshader);
            Gl.GetShader(vshader, ShaderParameterName.CompileStatus, out int vStatus);
            if (vStatus != (int)GLEnum.True)
                throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(vshader));

            Gl.ShaderSource(fshader, ReadShader("FragmentShader.frag"));
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
            if (key == Key.C)
            {
                cameraInFront = !cameraInFront;
            }
        }
        private static string ReadShader(string shaderFileName)
        {
            using (Stream shaderStream = typeof(Program).Assembly.GetManifestResourceStream("repulo_project.Shaders." + shaderFileName))
            using (StreamReader shaderReader = new StreamReader(shaderStream))
                return shaderReader.ReadToEnd();
        }

        private static void Window_Update(double deltaTime)
        {
            float dt = (float)deltaTime;

            foreach (var keyboard in inputContext.Keyboards)
            {
                bool turning = false;
                if (keyboard.IsKeyPressed(Key.W))
                {
                    var forward = Vector3D.Transform(new Vector3D<float>(0, 0, 1),
                        Quaternion<float>.CreateFromYawPitchRoll(planeYaw, planePitch, 0));
                    planePosition += forward * planeSpeed * dt;
                }
                if (keyboard.IsKeyPressed(Key.A))
                {
                    planeYaw += dt * 2f;
                    rollTarget = -maxRoll;
                    turning = true;
                }
                if (keyboard.IsKeyPressed(Key.D))
                {
                    planeYaw -= dt * 2f;
                    rollTarget = maxRoll;
                    turning = true;
                }
                if (keyboard.IsKeyPressed(Key.Up))
                {
                    planePitch += dt * 1f; // pitch up
                }
                if (keyboard.IsKeyPressed(Key.Down))
                {
                    planePitch -= dt * 1f; // pitch down
                }
                if (!turning)
                {
                    rollTarget = 0f;
                }
            }

            cameraDescriptor.TargetPosition = planePosition;
            planeRoll = Lerp(planeRoll, rollTarget, rollSpeed * dt);
            planeRoll = Clamp(planeRoll, -maxRoll, maxRoll);
            planePitch = Clamp(planePitch, -MathF.PI / 8f, MathF.PI / 8f);
            cameraDescriptor.PlaneYaw = planeYaw;
            cameraDescriptor.CameraInFront = cameraInFront;
            cubeArrangementModel.AdvanceTime(deltaTime);
            controller.Update((float)deltaTime);
        }

        private static unsafe void Window_Render(double deltaTime)
        {
            //Console.WriteLine($"Render after {deltaTime} [s].");

            // GL here
            Gl.Clear(ClearBufferMask.ColorBufferBit);
            Gl.Clear(ClearBufferMask.DepthBufferBit);

            Gl.UseProgram(program);

            SetViewMatrix();
            SetProjectionMatrix();

            SetLightColor();
            SetLightPosition();
            SetViewerPosition();
            SetShininess();

            DrawPlane();

            DrawSkyBox();
            DrawRings();
            //ImGuiNET.ImGui.ShowDemoWindow();
            ImGuiNET.ImGui.Begin("Lighting properties",
                ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar);
            ImGuiNET.ImGui.SliderFloat("Shininess", ref Shininess, 1, 200);
            ImGuiNET.ImGui.Checkbox("Draw only wireframe", ref DrawWireFrameOnly);
            ImGuiNET.ImGui.End();


            controller.Render();
        }

        private static unsafe void DrawSkyBox()
        {
            var rotation = Matrix4X4.CreateRotationZ((float)(Math.PI / 2));
            Matrix4X4<float> modelMatrix = Matrix4X4.CreateScale(400f) * rotation;
            SetModelMatrix(modelMatrix);
            Gl.BindVertexArray(skyBox.Vao);

            int textureLocation = Gl.GetUniformLocation(program, TextureUniformVariableName);
            if (textureLocation == -1)
            {
                throw new Exception($"{TextureUniformVariableName} uniform not found on shader.");
            }
            // set texture 0
            Gl.Uniform1(textureLocation, 0);

            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)GLEnum.Linear);
            Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)GLEnum.Linear);
            Gl.BindTexture(TextureTarget.Texture2D, skyBox.Texture.Value);

            Gl.DrawElements(GLEnum.Triangles, skyBox.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);

            CheckError();
            Gl.BindTexture(TextureTarget.Texture2D, 0);
            CheckError();
        }

        private static unsafe void DrawRings()
        {
            foreach (var ring in rings)
            {
                var translation = ring.RotationMatrix * Matrix4X4.CreateTranslation(ring.Position);
                SetModelMatrix(translation);
                Gl.BindVertexArray(ring.Torus.Vao);
                Gl.DrawElements(GLEnum.Triangles, ring.Torus.IndexArrayLength, GLEnum.UnsignedInt, null);
                Gl.BindVertexArray(0);
            }
        }

        private static unsafe void SetLightColor()
        {
            int location = Gl.GetUniformLocation(program, LightColorVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightColorVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 1f, 1f, 1f);
            CheckError();
        }

        private static unsafe void SetLightPosition()
        {
            int location = Gl.GetUniformLocation(program, LightPositionVariableName);

            if (location == -1)
            {
                throw new Exception($"{LightPositionVariableName} uniform not found on shader.");
            }

            Gl.Uniform3(location, 5f, 1f, 0f);
            //Gl.Uniform3(location, cameraDescriptor.Position.X, cameraDescriptor.Position.Y, cameraDescriptor.Position.Z);
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

        private static unsafe void DrawPlane()
        {
            var rotationYaw = Matrix4X4.CreateRotationY(planeYaw);
            var rotationPitch = Matrix4X4.CreateRotationX(planePitch);
            var rotationRoll = Matrix4X4.CreateRotationZ(planeRoll);
            var scale = Matrix4X4.CreateScale(0.05f);
            var translation = Matrix4X4.CreateTranslation(planePosition);

            var modelMatrix = scale * rotationRoll * rotationPitch * rotationYaw * translation;
            SetModelMatrix(modelMatrix);

            Gl.BindVertexArray(plane.Vao);
            Gl.DrawElements(GLEnum.Triangles, plane.IndexArrayLength, GLEnum.UnsignedInt, null);
            Gl.BindVertexArray(0);
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

            float[] face1Color = [1f, 0f, 0f, 1.0f];
            float[] face2Color = [0.0f, 1.0f, 0.0f, 1.0f];
            float[] face3Color = [0.0f, 0.0f, 1.0f, 1.0f];
            float[] face4Color = [1.0f, 0.0f, 1.0f, 1.0f];
            float[] face5Color = [0.0f, 1.0f, 1.0f, 1.0f];
            float[] face6Color = [1.0f, 1.0f, 0.0f, 1.0f];

            plane = ObjResourceReader.CreateTeapotWithColor(Gl, face1Color);


            skyBox = GlCube.CreateInteriorCube(Gl, "");

            glSphere = GlObject.CreateSphere(5.0f, Gl);

            glPlate = GlObject.CreateChalice(Gl);
            //1 x tengely
            //2 z tengely
            //3 y tengely
            //min max 200f
            var ring = new Ring(new Vector3D<float>(20f, 30f, 50f), 3f, GlObject.CreateTorus(Gl, 40f, 3f));
            ring.RotationMatrix = Matrix4X4.CreateRotationX(MathF.PI / 2);
            rings.Add(ring);
            var ring2 = new Ring(new Vector3D<float>(150f, 150f, 50f), 3f, GlObject.CreateTorus(Gl, 40f, 3f));
            ring.RotationMatrix = Matrix4X4.CreateRotationX(MathF.PI / 2);
            rings.Add(ring);
            var ring3 = new Ring(new Vector3D<float>(-150f, -150f, 100f), 3f, GlObject.CreateTorus(Gl, 40f, 3f));
            ring.RotationMatrix = Matrix4X4.CreateRotationX(MathF.PI / 2);
            rings.Add(ring);
            var ring4 = new Ring(new Vector3D<float>(0f, 150f, -100f), 3f, GlObject.CreateTorus(Gl, 40f, 3f));
            ring.RotationMatrix = Matrix4X4.CreateRotationX(MathF.PI / 2);
            rings.Add(ring);
            var ring5 = new Ring(new Vector3D<float>(50f, 70f, -20f), 3f, GlObject.CreateTorus(Gl, 40f, 3f));
            ring.RotationMatrix = Matrix4X4.CreateRotationX(MathF.PI / 2);
            rings.Add(ring);
            rings.Add(ring2);
            rings.Add(ring3);
            rings.Add(ring4);
            rings.Add(ring5);
            rings.Add(new Ring(new Vector3D<float>(10f, 15f, 100f), 3f, GlObject.CreateTorus(Gl, 40f, 3f)));
            rings.Add(new Ring(new Vector3D<float>(-10f, 20f, 150f), 3f, GlObject.CreateTorus(Gl, 40f, 3f)));


        }



        private static void Window_Closing()
        {
            plane.ReleaseGlObject();
        }

        private static unsafe void SetProjectionMatrix()
        {
            var projectionMatrix = Matrix4X4.CreatePerspectiveFieldOfView<float>((float)Math.PI / 4f, 1024f / 768f, 0.1f, 1000);
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
    }
}