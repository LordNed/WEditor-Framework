using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor.Rendering
{
    public class MeshRenderer : BaseRenderer
    {
        private Shader m_shader;

        public override void Initialize()
        {
            CreateShaderFromFile("UnlitTexture", "Shaders/UnlitTexture.vert", "Shaders/UnlitTexture.frag");
        }

        private void CreateShaderFromFile(string name, string vertShaderPath, string fragShaderPath)
        {
            m_shader = new Shader();
            m_shader.Name = name;

            // Initialize an OpenGL program for the shader.
            m_shader.ProgramId = GL.CreateProgram();

            int vertShaderId, fragShaderId;
            LoadShader(vertShaderPath, ShaderType.VertexShader, m_shader.ProgramId, out vertShaderId);
            LoadShader(fragShaderPath, ShaderType.FragmentShader, m_shader.ProgramId, out fragShaderId);

            // Once the shaders are created and linked to the program we can technically delete the shader.
            // This de-increments the internal reference count so that only the Program is holding onto it.
            // That way, once we delete the Program, it'll destroy itself.
            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            GL.BindAttribLocation(m_shader.ProgramId, (int)ShaderAttributeIds.Position, "vertexPos");

            // Link the program now that we've compiled it.
            GL.LinkProgram(m_shader.ProgramId);

            if (GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[WEditor.Core] Error linking shader. Result: {0}", GL.GetProgramInfoLog(m_shader.ProgramId));
                return;
            }

            m_shader.UniformMVP = GL.GetUniformLocation(m_shader.ProgramId, "modelview");

            if(GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[WEditor.Core] Failed to get modelview uniform for shader. Result: {0}", GL.GetProgramInfoLog(m_shader.ProgramId));
                return;
            }
        }

        public override void Render(Camera camera)
        {
            m_shader.Use();

            GL.Enable(EnableCap.DepthTest);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Clear any previously bound buffer.
            Matrix4 viewProjMatrix = camera.ViewMatrix * camera.ProjMatrix;

            // Bind the object's buffers, enable attributes and set layout.
            // 
        }
    }
}
