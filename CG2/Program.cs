using System;
using System.Diagnostics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PyramidDemo
{
    public class PyramidWindow : GameWindow
    {
        private readonly string VertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            out vec3 vertexColor;

            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                vertexColor = aColor;
            }
        ";

        private readonly string FragmentShaderSource = @"
            #version 330 core
            in vec3 vertexColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(vertexColor, 1.0);
            }
        ";

        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _shaderProgram;

        private float _rotationX = 0.0f;
        private float _rotationY = 0.0f;
        private float _rotationZ = 0.0f;
        private float _fov = 70.0f;

        // Время
        private Stopwatch _timer = new Stopwatch();

        public PyramidWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            // пирамида
            float[] vertices = {
                -0.25f, -0.25f, 0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.25f, -0.25f, 0.0f, 1.0f, 0.0f,
                0.25f, -0.25f, 0.0f, 1.0f, 0.0f, 1.0f,
                -0.25f, -0.25f, -0.5f, 0.5f, 0.25f, 0.0f,
                0.25f, -0.25f, -0.5f, 1.0f, 0.0f, 1.0f
            };

            uint[] indices = {
                0, 1, 2,
                0, 3, 1,
                3, 1, 4,
                4, 1, 2,
                0, 3, 4,
                0, 4, 2
            };


            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            int elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec3 aColor;
                out vec3 ourColor;
                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;
                void main(){
                    gl_Position = projection * view * model * vec4(aPosition, 1.0);
                    ourColor = aColor;
                }
            ");
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, @"
                #version 330 core
                in vec3 ourColor;
                out vec4 FragColor;
                void main(){
                    FragColor = vec4(ourColor, 1.0);
                }
            ");
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);
            CheckProgramLinking(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            int vertexLocation = GL.GetAttribLocation(_shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            int colorLocation = GL.GetAttribLocation(_shaderProgram, "aColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.Enable(EnableCap.DepthTest);

            _timer.Start();
        }

        private void CheckShaderCompilation(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"shader compile error: {info}");
            }
        }

        private void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Console.WriteLine($"program linking error: {info}");
            }
        }

        protected override void OnUnload()
        {
            base.OnUnload();

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteProgram(_shaderProgram);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Z) && (_fov <= 150.0f))
                _fov += 66 * (float)args.Time;

            if (input.IsKeyDown(Keys.V) && (_fov >= 15.0f))
                _fov -= 66 * (float)args.Time;

            if (input.IsKeyDown(Keys.W))
                _rotationX += 100 * (float)args.Time;

            if (input.IsKeyDown(Keys.S))
                _rotationX -= 100 * (float)args.Time;

            if (input.IsKeyDown(Keys.A))
                _rotationY += 100 * (float)args.Time;

            if (input.IsKeyDown(Keys.D))
                _rotationY -= 100 * (float)args.Time;

            if (input.IsKeyDown(Keys.Q))
                _rotationZ += 100 * (float)args.Time;

            if (input.IsKeyDown(Keys.E))
                _rotationZ -= 100 * (float)args.Time;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_shaderProgram);

            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(_rotationX))
                           * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotationY))
                           * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_rotationZ));

            Matrix4 view = Matrix4.LookAt(new Vector3(2.0f, 2.0f, 2.0f), Vector3.Zero, Vector3.UnitY);

            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov),
                                                                      Size.X / (float)Size.Y,
                                                                      0.1f,
                                                                      100.0f);

            int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int viewLoc = GL.GetUniformLocation(_shaderProgram, "view");
            GL.UniformMatrix4(viewLoc, false, ref view);

            int projectionLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            GL.UniformMatrix4(projectionLoc, false, ref projection);


            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, 18, DrawElementsType.UnsignedInt, 0);

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(0.0, 0.0, 1.0, 0.0);
                }
            ";

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            SwapBuffers();
        }

        [STAThread]
        public static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1000, 1000),
                Title = "ЛР 2",
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new PyramidWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.Run();
            }
        }
    }
}