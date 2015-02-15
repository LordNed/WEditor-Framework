using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WindViewer.Editor;
using System.Diagnostics;

namespace TestViewport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            //This just hooks winforms to draw our control.
            Application.Idle += Application_Idle;

            glControl1.KeyDown += HandleEventKeyDown;
            glControl1.KeyUp += HandleEventKeyUp;
            glControl1.MouseDown += HandleEventMouseDown;
            glControl1.MouseMove += HandleEventMouseMove;
            glControl1.MouseUp += HandleEventMouseUp;
            glControl1.Resize += Display.Internal_EventResize;
        }

        public enum ShaderAttributeIds
        {
            Position, Color,
            TexCoord
        }


        //The OpenGL "program" 's id. You use this to tell the GPU which Vertex/Fragment shader to use.
        private int _programId;

        //This is an id to a "Uniform". A uniform is set each frame and is constant for the entire drawcall.
        private int _uniformMVP;

        //This is an identifier that points to a specific buffer in GPU memory. You'd realistically need one of them
        //per object you're drawing, but I'm only drawing one atm so meh.
        private int _glVbo;
        private int _glEbo;

        // Test
        private Vector3 _positionHandle = new Vector3(5, 0, 0);
        private Camera _camera;

        /// <summary> Used to calculate the delta time of each processed frame. </summary>
        private Stopwatch _dtStopWatch;

        private Rendering.DebugRenderer m_debugRenderer;

        /// <summary>
        /// This is a helper function to load shaders from a file on the hard drive.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="type"></param>
        /// <param name="program"></param>
        /// <param name="address"></param>
        protected void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            //Gets an id from OpenGL
            address = GL.CreateShader(type);
            using (var streamReader = new StreamReader(fileName))
            {
                GL.ShaderSource(address, streamReader.ReadToEnd());
            }
            //Compiles the shader code
            GL.CompileShader(address);
            //Tells OpenGL that this shader (be it vertex of fragment) belongs to the specified program
            GL.AttachShader(program, address);

            //Error checking.
            int compileSuccess;
            GL.GetShader(address, ShaderParameter.CompileStatus, out compileSuccess);

            if (compileSuccess == 0)
                Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            /* This stuff is done *once* per program load */

            //Generate a Program ID
            _programId = GL.CreateProgram();

            //Create the Vertex and Fragment shader from file using our helper function
            int vertShaderId, fragShaderId;
            LoadShader("vs.glsl", ShaderType.VertexShader, _programId, out vertShaderId);
            LoadShader("fs.glsl", ShaderType.FragmentShader, _programId, out fragShaderId);

            //Deincriment the reference count on the shaders so that they don't exist until the context is destroyed.
            //(Housekeeping really)
            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            //This specifically tells OpenGL that we want to be able to refer to the "vertexPos" variable inside of the vs.glsl 
            //This allows us to later refer to it by specicic number.
            GL.BindAttribLocation(_programId, (int)ShaderAttributeIds.Position, "vertexPos");

            //Linking the shader tells OpenGL to finish compiling it or something. It's required. :P
            GL.LinkProgram(_programId);

            //Now that the program is linked we can get the identifier/location of the uniforms (by id) within the shader.
            _uniformMVP = GL.GetUniformLocation(_programId, "modelview");

            //More error checking
            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine(GL.GetProgramInfoLog(_programId));

            //The code in the following function would have to be done per-object
            CreateBuffersForObject();

            _camera = new Camera();
            _camera.ClearColor = Color.Aqua;
            _camera.transform.Position = new Vector3(0, 0, 10);


            Timer t1 = new Timer();
            t1.Interval = 16; //60-ish FPS
            t1.Tick += (o, args) =>
            {
                // Test
                Point mousePos = glControl1.PointToClient(Cursor.Position);
                Input.Internal_SetMousePos(new Vector2(mousePos.X, mousePos.Y));
                textBox1.Text = string.Format("Pos: {0}, {1}, Delta: {2}, {3}",
                    Input.MousePosition.X, Input.MousePosition.Y, Input.MouseDelta.X, Input.MouseDelta.Y);
            };
            t1.Enabled = true;

            _dtStopWatch = new Stopwatch();

            m_debugRenderer = new Rendering.DebugRenderer();
            m_debugRenderer.Initialize();
        }

        private void HandleEventKeyDown(object sender, KeyEventArgs e)
        {
            Input.Internal_SetKeyState(e.KeyCode, true);
        }

        private void HandleEventKeyUp(object sender, KeyEventArgs e)
        {
            Input.Internal_SetKeyState(e.KeyCode, false);
        }

        private void HandleEventMouseDown(object sender, MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, true);
        }

        private void HandleEventMouseMove(object sender, MouseEventArgs e)
        {
            //Input.Internal_SetMousePos(new Vector2(e.X, e.Y));
        }


        private void HandleEventMouseUp(object sender, MouseEventArgs e)
        {
            Input.Internal_SetMouseBtnState(e.Button, false);
        }


        private void CreateBuffersForObject()
        {
            //This is our vertex data, just positions
            Vector3[] meshVerts = 
            { 
                new Vector3(-1f, -1f,  -1f),
                new Vector3(1f, -1f,  -1f),
                new Vector3(1f, 1f,  -1f),
                new Vector3(-1f, 1f,  -1f),
                new Vector3(-1f, -1f,  1f),
                new Vector3(1f, -1f,  1f),
                new Vector3(1f, 1f,  1f),
                new Vector3(-1f, 1f,  1f),
            };

            //These are indexes (like the collision mesh uses)
            int[] meshIndexes =
            {
                //front
                0, 7, 3,
                0, 4, 7,
                //back
                1, 2, 6,
                6, 5, 1,
                //left
                0, 2, 1,
                0, 3, 2,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //bottom
                0, 1, 5,
                0, 5, 4
            };

            //Generate a buffer on the GPU and get the ID to it
            GL.GenBuffers(1, out _glVbo);

            //This "binds" the buffer. Once a buffer is bound, all actions are relative to it until another buffer is bound.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glVbo);

            //This uploads data to the currently bound buffer from the CPU -> GPU. This only needs to be done with the data changes (ie: you edited a vertexes position on the cpu side)
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(meshVerts.Length * Vector3.SizeInBytes), meshVerts,
                BufferUsageHint.StaticDraw);

            //Now we're going to repeat the same process for the Element buffer, which is what OpenGL calls indicies. (Notice how it's basically identical?)
            GL.GenBuffers(1, out _glEbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _glEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(meshIndexes.Length * 4), meshIndexes,
                BufferUsageHint.StaticDraw);
        }

        private void RenderFrame()
        {
            // Calculate a new DeltaTime for this frame (time it took the last frame to render).
            Time.Internal_UpdateTime(_dtStopWatch.ElapsedMilliseconds / 1000f);
            _dtStopWatch.Restart();





            // Reset the Handles at the start of the frame.
            HandleUtility.s_NearestControl = -1;
            HandleUtility.s_NearestDistance = float.MaxValue;


            Event newEvent = new Event();
            newEvent.mousePosition = Input.MousePosition;
            newEvent.delta = Input.MouseDelta.Xy;
            newEvent.delta.Y = -newEvent.delta.Y; //0,0 is bottom left, so we negate things.

            // Run a layout event first to register all Position handles
            Event.current = newEvent;
            newEvent.Type = EventType.Layout;
            _positionHandle = Handles.DoPositionHandle(_positionHandle);
            

            // Then run the mouse up/drag/down events.
            if (Input.GetMouseButtonDown(0))
            {
                newEvent.Type = EventType.MouseDown;
                newEvent.button = 0;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                newEvent.Type = EventType.MouseUp;
                newEvent.button = 0;
            }

            if (Input.GetMouseButton(0) && !(Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)))
            {
                newEvent.Type = EventType.MouseDrag;
                newEvent.button = 0;
            }

            Event.current = newEvent;

            // run the handles again...
            _positionHandle = Handles.DoPositionHandle(_positionHandle);

            Rendering.DebugRenderer.DrawLine(_positionHandle, _positionHandle - Vector3.UnitX * 2);
            Rendering.DebugRenderer.DrawLine(_positionHandle, _positionHandle + Vector3.UnitZ * 2);
            Rendering.DebugRenderer.DrawLine(_positionHandle, _positionHandle + Vector3.UnitY * 2);

            if (Input.GetKeyDown(Keys.Space))
            {
                Console.WriteLine("Break!");
            }
            Draw();

            // Calculate the input for this frame (calculate if a button was clicked/released/held, etc.)
            Input.Internal_UpdateInputState();
            m_debugRenderer.PostRenderUpdate();
        }

        private void Draw()
        {
            //This is called *every* frame. Every time we want to draw, we do the following.
            GL.ClearColor(_camera.ClearColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Tell OpenGL which program to use (our Vert Shader (VS) and Frag Shader (FS))
            GL.UseProgram(_programId);

            //Enable depth-testing which keeps models from rendering inside out.
            GL.Enable(EnableCap.DepthTest);

            //Clear any previously bound buffer so we have no leftover data or anything.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            /* Anything below this point would technically be done per object you draw */

            //Build a Model View Projection Matrix. This is where you would add camera movement (modifiying the View matrix), Perspective rendering (perspective matrix) and model position/scale/rotation (Model)
            Matrix4 viewMatrix = Matrix4.LookAt(new Vector3(25, 15, 25), Vector3.Zero, Vector3.UnitY);
            Matrix4 projMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(65), glControl1.Width / (float)glControl1.Height, 10, 1000);

            Matrix4 modelMatrix = Matrix4.CreateTranslation(_positionHandle); //Matrix4.Identity); //Identity = doesn't change anything when multiplied.

            //Bind the buffers that have the data you want to use
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glVbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _glEbo);

            //Then, you have to tell the GPU what the contents of the Array buffer look like. Ie: Is each entry just a position, or does it have a position, color, normal, etc.
            GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
            GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);

            //Upload the WVP to the GPU
            Matrix4 finalMatrix = modelMatrix * _camera.GetViewProjMatrix(); // viewMatrix* projMatrix;
            GL.UniformMatrix4(_uniformMVP, false, ref finalMatrix);

            //Now we tell the GPU to actually draw the data we have
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            //This is cleanup to undo the changes to the OpenGL state we did to draw this model.
            GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);

            m_debugRenderer.Render(_camera);


            /* This anything below is done at the end of the frame, only once */
            glControl1.SwapBuffers();
        }

        void Application_Idle(object sender, EventArgs e)
        {
            while (glControl1.IsIdle)
            {
               RenderFrame();
            }
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            //RenderFrame();
        }
    }
}
