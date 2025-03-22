using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

class CubeGame : GameWindow
{
    private int vertexBufferObject;
    private int vertexArrayObject;
    private int shaderProgram;
    private int elementBufferObject;
    private float rotation = 0.0f;
    private Vector3 cubePosition = Vector3.Zero; // Posición inicial del cubo
    private float moveSpeed = 5.0f; // Velocidad de movimiento

    private readonly float[] vertices = {
        // Posiciones        // Colores (RGB)
        // Columna izquierda
        -0.5f, -0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
        -0.3f, -0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
        -0.3f,  0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
        -0.5f,  0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
        -0.5f, -0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
        -0.3f, -0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
        -0.3f,  0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
        -0.5f,  0.5f,  0.2f,  0.0f, 1.0f, 0.0f, 

        // Columna derecha
         0.3f, -0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
         0.5f, -0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
         0.5f,  0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
         0.3f,  0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
         0.3f, -0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
         0.5f, -0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
         0.5f,  0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
         0.3f,  0.5f,  0.2f,  0.0f, 1.0f, 0.0f, 

        // Base de la U
        -0.5f, -0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
         0.5f, -0.5f, -0.2f,  0.0f, 1.0f, 0.0f,
         0.5f, -0.3f, -0.2f,  0.0f, 1.0f, 0.0f,
        -0.5f, -0.3f, -0.2f,  0.0f, 1.0f, 0.0f,
        -0.5f, -0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
         0.5f, -0.5f,  0.2f,  0.0f, 1.0f, 0.0f,
         0.5f, -0.3f,  0.2f,  0.0f, 1.0f, 0.0f,
        -0.5f, -0.3f,  0.2f,  0.0f, 1.0f, 0.0f
    };

    private readonly uint[] indices = {
        // Caras columna izquierda
        0, 1, 2, 2, 3, 0,
        4, 5, 6, 6, 7, 4,
        0, 1, 5, 5, 4, 0,
        2, 3, 7, 7, 6, 2,
        0, 3, 7, 7, 4, 0,
        1, 2, 6, 6, 5, 1,

        // Caras columna derecha
        8, 9, 10, 10, 11, 8,
        12, 13, 14, 14, 15, 12,
        8, 9, 13, 13, 12, 8,
        10, 11, 15, 15, 14, 10,
        8, 11, 15, 15, 12, 8,
        9, 10, 14, 14, 13, 9,

        // Caras base
        16, 17, 18, 18, 19, 16,
        20, 21, 22, 22, 23, 20,
        16, 17, 21, 21, 20, 16,
        18, 19, 23, 23, 22, 18,
        16, 19, 23, 23, 20, 16,
        17, 18, 22, 22, 21, 17
    };

    public CubeGame() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        Size = new Vector2i(800, 600);
        Title = "Grafica U en 3D";
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);

        vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObject);

        //carga los vertices
        vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        //carga los indices
        elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 aPosition;
            layout (location = 1) in vec3 aColor;
            out vec3 fragColor;
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            void main()
            {
                gl_Position = projection * view * model * vec4(aPosition, 1.0);
                fragColor = aColor;
            }";

        string fragmentShaderSource = @"
            #version 330 core
            in vec3 fragColor;
            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(fragColor, 1.0);
            }";

        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);
        GL.UseProgram(shaderProgram);

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }

    private Vector3 lastPosition = Vector3.Zero; // Guardar la última posición
    private bool isMoving = false; // Flag para saber si la figura se está moviendo

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        // Guardar la posición anterior del cubo
        Vector3 previousPosition = cubePosition;

        // Mover el cubo en función de las teclas presionadas
        if (KeyboardState.IsKeyDown(Keys.W)) cubePosition.Z -= moveSpeed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.S)) cubePosition.Z += moveSpeed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.A)) cubePosition.X -= moveSpeed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.D)) cubePosition.X += moveSpeed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.Q)) cubePosition.Y -= moveSpeed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.E)) cubePosition.Y += moveSpeed * (float)args.Time;

        // Verificar si la figura se está moviendo
        if (cubePosition != previousPosition)
        {
            isMoving = true; // La figura se está moviendo
            lastPosition = cubePosition; // Guardar la última posición
        }
        else if (isMoving)
        {
            // Si la figura dejó de moverse, imprimir la última posición
            Console.WriteLine("Posición actual: " + lastPosition);
            isMoving = false; // Reiniciar el flag
        }
    }
    //configura matrices y dibuja
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        Matrix4 model = Matrix4.CreateTranslation(cubePosition);
        //Par ver
        Matrix4 view = Matrix4.LookAt(new Vector3(2, 2, 4), Vector3.Zero, Vector3.UnitY);
        Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);

        int modelLocation = GL.GetUniformLocation(shaderProgram, "model");
        GL.UniformMatrix4(modelLocation, false, ref model);

        int viewLocation = GL.GetUniformLocation(shaderProgram, "view");
        GL.UniformMatrix4(viewLocation, false, ref view);

        int projectionLocation = GL.GetUniformLocation(shaderProgram, "projection");
        GL.UniformMatrix4(projectionLocation, false, ref projection);

        GL.UseProgram(shaderProgram);
        GL.BindVertexArray(vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }

    //manejo de ventanas
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
        GL.DeleteBuffer(vertexBufferObject);
        GL.DeleteBuffer(elementBufferObject);
        GL.DeleteVertexArray(vertexArrayObject);
        GL.DeleteProgram(shaderProgram);
        base.OnUnload();
    }

    static void Main()
    {
        using (var game = new CubeGame())
        {
            game.Run();
        }
    }
}
