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

            m_shader = CreateShaderFromFile("UnlitTexture", "Shaders/UnlitTexture.vert", "Shaders/UnlitTexture.frag");
        }
                

        public override void Render(Camera camera)
        {
            m_shader.Use();

            GL.Enable(EnableCap.DepthTest);
            //GL.Disable(EnableCap.CullFace);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Clear any previously bound buffer.
            Matrix4 viewProjMatrix = camera.ViewMatrix * camera.ProjMatrix;

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
