using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace WEditor.Rendering
{
    public class DebugRenderer : BaseRenderer
    {
        #region Private Classes
        private class MeshInstance
        {
            public MeshInstance(Vector3 pos, Color color, Quaternion rot, Vector3 scale, float lifetime)
            {
                Position = pos;
                Rotation = rot;
                Scale = scale;
                Color = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
                Lifetime = lifetime;
            }

            public Vector3 Position { get; private set; }
            public Quaternion Rotation { get; private set; }
            public Vector3 Scale { get; private set; }
            public Vector4 Color { get; private set; }
            public float Lifetime;
        }

        private class LineInstance : IDisposable
        {
            private int _vboId;

            public Vector4 Color;
            public float Lifetime;

            public LineInstance(Vector3 startPos, Vector3 endPos, Color color, float lifetime)
            {
                Vector3[] verts = new[] { startPos, endPos };
                Color = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255F);
                Lifetime = lifetime;

                GL.GenBuffers(1, out _vboId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verts.Length * 4 * 3), verts, BufferUsageHint.StaticDraw);
            }

            public void Dispose()
            {
                GL.DeleteBuffer(_vboId);
            }

            public void Bind()
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboId);
            }

            public void Draw()
            {
                GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            }
        }

        #endregion

        private static DebugRenderer m_instance;
        private Dictionary<Mesh, List<MeshInstance>> m_meshRenderList;
        private List<LineInstance> m_lineRenderList;
        private Shader m_shader;

        // Instances of Geometry


        public override void Initialize()
        {
            m_meshRenderList = new Dictionary<Mesh, List<MeshInstance>>();
            m_lineRenderList = new List<LineInstance>();
            m_instance = this;

            m_shader = CreateShaderFromFile("DebugDraw", "Shaders/DebugDraw.vert", "Shaders/DebugDraw.frag");
        }

        public override void Render(Camera camera)
        {
            m_shader.Use();

            GL.Enable(EnableCap.DepthTest);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            Matrix4 viewProjMatrix = camera.ViewMatrix * camera.ProjMatrix;

            foreach(var meshType in m_meshRenderList)
            {
                Mesh mesh = meshType.Key;

                // Bind the object's buffers
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.m_glVbo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, mesh.m_glEbo);

                // Set layout and enable attributes.
                GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);
                GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, 3 * 4, 0);


                foreach(var instance in meshType.Value)
                {
                    Matrix4 worldMatrix = Matrix4.CreateScale(instance.Scale) * Matrix4.CreateFromQuaternion(instance.Rotation) * Matrix4.CreateTranslation(instance.Position);
                    Matrix4 finalMatrix = worldMatrix * viewProjMatrix;

                    // Upload Matrix to GPU
                    GL.UniformMatrix4(m_shader.UniformMVP, false, ref finalMatrix);

                    // Upload the primitives color
                    GL.Uniform3(m_shader.UniformColor, instance.Color.Xyz);

                    // Draw it.
                    GL.DrawArrays(PrimitiveType.TriangleStrip, mesh.Triangles.Length, 0);
                }

                GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);
            }

            // The line instances are all drawn in WorldSpace and thus don't require an individual
            // Model matrix. As a result we just upload the ViewProj matrix to the GPU.
            GL.UniformMatrix4(m_shader.UniformMVP, false, ref viewProjMatrix);
            foreach(var lineInstance in m_lineRenderList)
            {
                // Bind that buffer.
                lineInstance.Bind();

                GL.VertexAttribPointer((int)ShaderAttributeIds.Position, 3, VertexAttribPointerType.Float, false, 3 * 4, 0);
                GL.EnableVertexAttribArray((int)ShaderAttributeIds.Position);

                //Upload the primitives color.
                GL.Uniform3(m_shader.UniformColor, lineInstance.Color.Xyz);

                lineInstance.Draw();

                GL.DisableVertexAttribArray((int)ShaderAttributeIds.Position);
            }
        }

        public override void PostRenderUpdate()
        {
            // Remove expired Mesh renderings
            foreach (var type in m_meshRenderList)
            {
                List<MeshInstance> pendRemoval = new List<MeshInstance>();
                foreach (var instance in type.Value)
                {
                    instance.Lifetime -= Time.DeltaTime;
                    if (instance.Lifetime <= 0f)
                        pendRemoval.Add(instance);
                }

                foreach (MeshInstance instance in pendRemoval)
                {
                    type.Value.Remove(instance);
                }
            }

            // Remove expired Line renderings.
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

        public static void DrawLine(Vector3 startPos, Vector3 endPos, float duration = 0f)
        {
            DrawLine(startPos, endPos, Color.White, duration);
        }
        public static void DrawLine(Vector3 startPos, Vector3 endPos, Color color, float duration = 0f)
        {
            m_instance.m_lineRenderList.Add(new LineInstance(startPos, endPos, color, duration));
        }

        //public static void DrawWireCube(Vector3 position, Color color, Quaternion rotation, Vector3 scale)
        //{
        //    if (!m_instance.m_meshRenderList.ContainsKey(_instance._cubeMeshWire))
        //        m_instance.m_meshRenderList.Add(_instance._cubeMeshWire, new List<Instance>());

        //    m_instance.m_meshRenderList[_instance._cubeMeshWire].Add(new MeshInstance(position, color, rotation, scale, 0f));
        //}

        //public static void DrawCube(Vector3 position, Color color, Quaternion rotation, Vector3 scale)
        //{
        //    if (!_instance._renderList.ContainsKey(_instance._cubeMeshSolid))
        //        _instance._renderList.Add(_instance._cubeMeshSolid, new List<Instance>());

        //    _instance._renderList[_instance._cubeMeshSolid].Add(new Instance(position, color, rotation, scale, 0f));
        //}
    }
}
