using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RectangleTransformations
{
    class Program
    {
        static void Main(string[] args)
        {
            var nativeSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1000, 1000),
                Title = "ЛР 1",
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new RectangleWindow(GameWindowSettings.Default, nativeSettings))
            {
                window.Run();
            }
        }
    }

    public class RectangleWindow : GameWindow
    {
        private int _shaderProgram;
        private int _vao;
        private int _vbo;
        private Vector2 _position = Vector2.Zero;
        private float _rotation = 0.0f;
        private Vector2 _scale = Vector2.One;
       private Vector3 _color = new Vector3(0.0f, 0.0f, 1.0f);

        private readonly string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec2 aPosition;

            uniform mat4 model;

            void main()
            {
                gl_Position = model * vec4(aPosition, 0.0, 1.0);
            }
        ";

        private readonly string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;

            uniform vec3 ourColor;

            void main()
            {
                FragColor = vec4(ourColor, 1.0);
            }
        ";

       private readonly float[] vertices = {
            -0.5f, -0.25f,
             0.5f, -0.25f,
             0.5f,  0.25f,

             0.5f,  0.25f,
            -0.5f,  0.25f,
            -0.5f, -0.25f
        };

        public RectangleWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);
            CheckShaderCompilation(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);
            CheckShaderCompilation(fragmentShader);

            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);
            CheckProgramLinking(_shaderProgram);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_shaderProgram);

            Matrix4 model = Matrix4.CreateScale(new Vector3(_scale.X, _scale.Y, 1.0f)) *
                            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(_rotation)) *
                            Matrix4.CreateTranslation(new Vector3(_position.X, _position.Y, 0.0f));

            int modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
            GL.UniformMatrix4(modelLoc, false, ref model);

            int colorLoc = GL.GetUniformLocation(_shaderProgram, "ourColor");
            GL.Uniform3(colorLoc, _color);

            GL.BindVertexArray(_vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            SwapBuffers();

        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var input = KeyboardState;

            _color = new Vector3(0.0f, 0.0f, 1.0f);;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            // движение по канвасу + изменение цвета
            if (input.IsKeyDown(Keys.W) && (_position.Y < 1))
            {
                _position.Y += 0.66f * (float)args.Time;
                _color = new Vector3(1.0f, 0.0f, 0.0f); //вверх - красный
            }
            if (input.IsKeyDown(Keys.S) && (_position.Y > -1))
            {
                _position.Y -= 0.66f * (float)args.Time;
                _color = new Vector3(0.0f, 1.0f, 0.0f); // вниз - зеленый
            }
            if (input.IsKeyDown(Keys.A) && (_position.X > -1))
            {
                _position.X -= 0.66f * (float)args.Time;
                _color = new Vector3(1.0f, 1.0f, 0.0f); // влево - желтый
            }
            if (input.IsKeyDown(Keys.D) && (_position.X < 1))
            {
                _position.X += 0.66f * (float)args.Time;
                _color = new Vector3(1.0f, 0.0f, 1.0f); // вправо - розовый
            }

            if (input.IsKeyDown(Keys.Q))
            {
                _rotation += 90.0f * (float)args.Time;
                _color = new Vector3(1.0f, 0.84f, 0.0f); // против часовой - золотой
            }
            if (input.IsKeyDown(Keys.E))
            {
                _rotation -= 90.0f * (float)args.Time;
                _color = new Vector3(0.5f, 0.25f, 0.0f); // по часовой - коричневый
            }


            if (input.IsKeyDown(Keys.Z))
            {
                _scale += new Vector2(0.66f, 0.66f) * (float)args.Time;
                _color = new Vector3(0.0f, 1.0f, 1.0f); // бирюзовый при увеличении
            }
            if (input.IsKeyDown(Keys.X))
            {
                _scale -= new Vector2(0.66f, 0.66f) * (float)args.Time;
                _scale = Vector2.ComponentMax(_scale, new Vector2(0.4f, 0.4f));
                _color = new Vector3(0.5f, 0.5f, 0.5f); // серый - при уменьшении
            }

        }

        protected override void OnUnload()
        {
            base.OnUnload();

            // Clean up resources
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shaderProgram);
        }

        private void CheckShaderCompilation(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                Console.WriteLine($"Error compiling shader: {infoLog}");
            }
        }

        private void CheckProgramLinking(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(program);
                Console.WriteLine($"Error linking program: {infoLog}");
            }
        }
    }
}