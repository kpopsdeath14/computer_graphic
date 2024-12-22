using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;

namespace PyramidFlightMode
{
    public class Program
    {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1000, 1000),
                Title = "ЛР №;3",
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                var game = new Game(window);
                game.Run();
            }
        }
    }

    public class Game
    {
        private GameWindow _window;

        private Matrix4 _model;
        private Matrix4 _view;
        private Matrix4 _projection;

        private int _shaderProgram;

        private int _vbo;
        private int _vao;

        private bool fly = false;

        private float[] _vertices;
        private Vector3 _cameraPosition = new Vector3(0, 0, 5);
        private Vector3 _cameraFront = new Vector3(0, 0, -1);
        private Vector3 _cameraUp = Vector3.UnitY;

        private float _cameraSpeed = 8.0f;

        private Vector2 _lastMousePosition;
        private bool moved = true;
        private float _yaw = -90f;
        private float _pitch = 0f;
        private float _sensitivity = 0.1f;

        public Game(GameWindow window)
        {
            _window = window;
            _window.Load += OnLoad;
            _window.RenderFrame += OnRenderFrame;
            _window.UpdateFrame += OnUpdateFrame;
            _window.Resize += OnResize;
        }

        private void OnLoad()
        {
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            
            _shaderProgram = CreateShaderProgram();
            _vertices = cubeVertices();
            _vbo = GL.GenBuffer();
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);
            var positionLocation = GL.GetAttribLocation(_shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _window.Size.X / (float)_window.Size.Y, 0.1f, 100f);
            GL.Enable(EnableCap.DepthTest);

            _window.CursorState = CursorState.Grabbed;
        }

        private void OnUpdateFrame(FrameEventArgs args)
        {
            float deltaTime = (float)args.Time;

            var input = _window.KeyboardState;

            if (input.IsKeyDown(Keys.L) && (fly == false))
                fly = true;
            
            if (fly)
                _cameraPosition += _cameraFront * _cameraSpeed * deltaTime;

            if (input.IsKeyDown(Keys.W))
                _cameraPosition += _cameraFront * _cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.S))
                _cameraPosition -= _cameraFront * _cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.A))
                _cameraPosition -= Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * _cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.D))
                _cameraPosition += Vector3.Normalize(Vector3.Cross(_cameraFront, _cameraUp)) * _cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.U))
                _cameraPosition += _cameraUp * _cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.J))
                _cameraPosition -= _cameraUp * _cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.P))
                fly = false;
            if (input.IsKeyDown(Keys.X))
                _window.CursorState = CursorState.Normal;

            var mouse = _window.MouseState;
            var mousePosition = new Vector2(mouse.X, mouse.Y);

            if (moved)
            {
                _lastMousePosition = mousePosition;
                moved = false;
            }
            else
            {
                var deltaX = mousePosition.X - _lastMousePosition.X;
                var deltaY = _lastMousePosition.Y - mousePosition.Y;

                _lastMousePosition = mousePosition;

                deltaX *= _sensitivity;
                deltaY *= _sensitivity;

                _yaw += deltaX;
                _pitch += deltaY;

                _pitch = MathHelper.Clamp(_pitch, -80f, 80f);

                Vector3 front;
                front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
                front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
                front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));

                _cameraFront = Vector3.Normalize(front);
            }

            _view = Matrix4.LookAt(_cameraPosition, _cameraPosition + _cameraFront, _cameraUp);
        }

        private void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(_shaderProgram);

            int modelLocation = GL.GetUniformLocation(_shaderProgram, "uModel");
            int viewLocation = GL.GetUniformLocation(_shaderProgram, "uView");
            int projectionLocation = GL.GetUniformLocation(_shaderProgram, "uProjection");

            GL.UniformMatrix4(viewLocation, false, ref _view);
            GL.UniformMatrix4(projectionLocation, false, ref _projection);

            GL.BindVertexArray(_vao);

            _model = Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 2.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 4.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 6.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(2.0f, 6.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(4.0f, 6.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(6.0f, 6.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(6.0f, 4.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(6.0f, 2.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(6.0f, 0.0f, 0.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 0.0f, -4.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 0.0f, -6.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 2.0f, -6.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 0.0f, -8.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 2.0f, -8.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

            _model = Matrix4.CreateTranslation(0.0f, 4.0f, -8.0f);
            GL.UniformMatrix4(modelLocation, false, ref _model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);

           

            _window.SwapBuffers();
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, _window.Size.X, _window.Size.Y);
            _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), _window.Size.X / (float)_window.Size.Y, 0.1f, 100f);
        }

        private int CreateShaderProgram()
        {
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;

                uniform mat4 uModel;
                uniform mat4 uView;
                uniform mat4 uProjection;

                void main()
                {
                    gl_Position = uProjection * uView * uModel * vec4(aPosition, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(0.0, 0.0, 1.0, 0.0);
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            CheckProgramLinking(shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private void CheckShaderCompilation(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                System.Console.WriteLine($"shader compiling error: {infoLog}");
            }
        }

        private void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                System.Console.WriteLine($"program linking error: {infoLog}");
            }
        }

        private float[] cubeVertices()
        {
            return new float[]
            {
                -1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f, 1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f,-1.0f,
                1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f,-1.0f,
                1.0f,-1.0f,-1.0f,
                1.0f, 1.0f,-1.0f,
                1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,
                1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,
                -1.0f,-1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,
                1.0f,-1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f,-1.0f,-1.0f,
                1.0f, 1.0f,-1.0f,
                1.0f,-1.0f,-1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f,-1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                1.0f, 1.0f,-1.0f,
                -1.0f, 1.0f,-1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f, 1.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,
                1.0f,-1.0f, 1.0f
            };
        }


        public void Run()
        {
            _window.Run();
        }
    }
}
