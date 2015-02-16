using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class StaticMeshRenderer : BaseRenderer
    {
        private List<MeshRenderer> m_meshList;
        private static StaticMeshRenderer m_instance;

        private Shader m_shader;

        public override void Initialize()
        {
            m_meshList = new List<MeshRenderer>();
            m_instance = this;

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
            //GL.Disable(EnableCap.CullFace);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Clear any previously bound buffer.
            Matrix4 viewProjMatrix = camera.ViewMatrix * (Matrix4.CreateScale(-1f, 1f, 1f) * camera.ProjMatrix);

            for(int i = 0; i < m_meshList.Count; i++)
            {
                MeshRenderer meshRenderer = m_meshList[i];
                Mesh mesh = meshRenderer.Mesh;
                Transform trans = meshRenderer.GetTransform();

                if (mesh == null)
                    continue;

                // Bind the object's buffers
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.m_glVbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.m_glEbo);

                // Set layout and enable attributes.
                GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
                GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, 3 * 4, 0);

                // Calculate a Model matrix.
                Matrix4 modelMatrix = Matrix4.CreateScale(trans.Scale) * Matrix4.CreateFromQuaternion(trans.Rotation) * Matrix4.CreateTranslation(trans.Position);
                Matrix4 finalMatrix = modelMatrix * viewProjMatrix;

                // Upload the MVP matrix to the GPU
                GL.UniformMatrix4(m_shader.UniformMVP, false, ref finalMatrix);

                // Draw it.
                GL.DrawElements(PrimitiveType.TriangleStrip, mesh.Triangles.Length, DrawElementsType.UnsignedInt, 0);

                // Disable the Attribute
                GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);
            }            
        }

        public static void RegisterMesh(MeshRenderer mesh)
        {
            m_instance.m_meshList.Add(mesh);
        }
    }
}
