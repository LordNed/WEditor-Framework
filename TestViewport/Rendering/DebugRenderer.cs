using System;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace TestViewport.Rendering
{
    public enum ShaderAttributeIds
    {
        Position, Color,
        TexCoord
    }

    public class Shader : IDisposable
    {
        public void Use()
        {
            GL.UseProgram(ProgramId);
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }

        public string Name;
        public int ProgramId;
        public int UniformMVP;
        public int UniformColor;
    }

    class DebugRenderer
    {
        private class LineInstance : IDisposable
        {
            public Vector4 Color { get; private set; }
            public float Lifetime;

            private int m_vboId;

            public LineInstance(Vector3 startPos, Vector3 endPos, Color color, float lifetime)
            {
                Vector3[] verts = new[] { startPos, endPos };
                Color = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255F);
                Lifetime = lifetime;

                GL.GenBuffers(1, out m_vboId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, m_vboId);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * 4 * 3), verts, BufferUsageHint.StaticDraw);
            }

            public void Dispose()
            {
                GL.DeleteBuffer(m_vboId);
            }

            public void Bind()
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, m_vboId);
            }

            public void Draw()
            {
                GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            }
        }

        private List<LineInstance> m_lineRenderList;
        private Shader m_shader;
        private static DebugRenderer m_instance;

        public void Initialize()
        {
            m_lineRenderList = new List<LineInstance>();
            CreateShaderFromFile("debug", "shaders/debug_vs.glsl", "shaders/debug_fs.glsl");
            m_instance = this;
        }

        protected void CreateShaderFromFile(string name, string vertShader, string fragShader)
        {
            m_shader = new Shader();
            m_shader.Name = name;

            //Initialize the OpenGL Program
            m_shader.ProgramId = GL.CreateProgram();

            int vertShaderId, fragShaderId;
            LoadShader(vertShader, ShaderType.VertexShader, m_shader.ProgramId, out vertShaderId);
            LoadShader(fragShader, ShaderType.FragmentShader, m_shader.ProgramId, out fragShaderId);

            //Deincriment the reference count on the shaders so that they 
            //don't exist until the context is destroyed.
            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            GL.BindAttribLocation(m_shader.ProgramId, (int)ShaderAttributeIds.Position, "vertexPos");

            //Link shaders 
            GL.LinkProgram(m_shader.ProgramId);

            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine(GL.GetProgramInfoLog(m_shader.ProgramId));

            m_shader.UniformMVP = GL.GetUniformLocation(m_shader.ProgramId, "modelview");
            m_shader.UniformColor = GL.GetUniformLocation(m_shader.ProgramId, "inColor");

            if (GL.GetError() != ErrorCode.NoError)
                Console.WriteLine(GL.GetProgramInfoLog(m_shader.ProgramId));
        }

        protected void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            using (var streamReader = new System.IO.StreamReader(fileName))
            {
                GL.ShaderSource(address, streamReader.ReadToEnd());
            }
            GL.CompileShader(address);
            GL.AttachShader(program, address);

            int compileSuccess;
            GL.GetShader(address, ShaderParameter.CompileStatus, out compileSuccess);

            if (compileSuccess == 0)
                Console.WriteLine(GL.GetShaderInfoLog(address));
        }

        public static void DrawLine(Vector3 startPos, Vector3 endPos, float duration = 0f)
        {
            DrawLine(startPos, endPos, Color.White, duration);
        }
        public static void DrawLine(Vector3 startPos, Vector3 endPos, Color color, float duration = 0f)
        {
            m_instance.m_lineRenderList.Add(new LineInstance(startPos, endPos, color, duration));
        }

        public void Render(Camera camera)
        {
            m_shader.Use();

            //State Muckin'
            GL.Enable(EnableCap.DepthTest);

            //Clear any previously bound buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            Matrix4 viewProjMatrix = camera.GetViewProjMatrix();


            //Now draw all of our debug lines
            //Upload the WVP to the GPU
            GL.UniformMatrix4(m_shader.UniformMVP, false, ref viewProjMatrix);
            foreach (LineInstance instance in m_lineRenderList)
            {
                //Bind the object buffer, enable attribs and set layout.
                instance.Bind();
                GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, 3 * 4, 0);
                GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);

                //Upload the primitives color.
                GL.Uniform3(m_shader.UniformColor, new Vector3(instance.Color.X, instance.Color.Y, instance.Color.Z));

                instance.Draw();


                GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);
            }
        }

        public void PostRenderUpdate()
        {
            List<LineInstance> linePendRemoval = new List<LineInstance>();
            foreach (var lineInstance in m_lineRenderList)
            {
                lineInstance.Lifetime -= Time.DeltaTime;

                if (lineInstance.Lifetime <= 0f)
                    linePendRemoval.Add(lineInstance);
            }
            foreach (LineInstance instance in linePendRemoval)
            {
                m_lineRenderList.Remove(instance);
                instance.Dispose();
            }
        }
    }
}
