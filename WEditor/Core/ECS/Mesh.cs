using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor
{
    public class Mesh : BaseComponent
    {
        /// <summary> The bounding volume of the mesh. This is an axis-aligned bounding box in local space. Setting Verticies will automatically update the Bounds. (Read Only) </summary>
        public Bounds Bounds { get; private set; }
        /// <summary> Vertex colors of the mesh. </summary>
        public Color[] Colors;
        /// <summary> Vertex normals of the mesh. </summary>
        public Vector3[] Normals;
        /// <summary> Triangle indexes of the mesh. </summary>
        public int[] Triangles {
            get { return m_meshTriangles; }
            set {
                m_meshTriangles = value;
                GenerateBuffers();
            }
        }
        /// <summary> Vertex positions of the mesh. </summary>
        public Vector3[] Verticies {
            get { return m_meshVerts; }
            set {
                m_meshVerts = value;
                VertexCount = m_meshVerts == null ? 0 : m_meshVerts.Length;
                RecalculateBounds();
                GenerateBuffers();
            }
        }

        /// <summary> Texture Coordinates of the mesh. </summary>
        public Vector2[] UVs;
        /// <summary> Vertex count of the mesh. (Read Only) </summary>
        public int VertexCount { get; private set; }
        /// <summary> MeshTopology lets you render complex things using lines or points instead of just triangles if desired. </summary>
        //public PrimitiveType MeshTopology;

        // Ugly hack till we get things drawing
        public int m_glVbo = -1;
        public int m_glEbo = -1;


        private Vector3[] m_meshVerts;
        private int[] m_meshTriangles;


        public Mesh()
        {
            Bounds = new Bounds(Vector3.Zero, Vector3.Zero);
            VertexCount = 0;
            Colors = null;
            Normals = null;
            Triangles = null;
            Verticies = null;
            UVs = null;
            //MeshTopology = PrimitiveType.Triangles;
        }

        public virtual void RecalculateBounds()
        {
            float minX = 0f, maxX = 0f, minY = 0f, maxY = 0f, minZ = 0f, maxZ = 0f;
            if(m_meshVerts == null)
            {
                Bounds = new Bounds(Vector3.Zero, Vector3.Zero);
                return;
            }

            // Run through all of the Vertices and determine a new Extents.
            for(int i = 0; i < m_meshVerts.Length; i++)
            {
                Vector3 point = m_meshVerts[i];
                if (point.X < minX)
                    minX = point.X;
                if (point.X > maxX)
                    maxX = point.X;
                if (point.Y < minY)
                    minY = point.Y;
                if (point.Y > maxY)
                    maxY = point.Y;
                if (point.Z < minZ)
                    minZ = point.Z;
                if (point.Z > maxZ)
                    maxZ = point.Z;
            }

            // Create a new bounds given the total size.
            Vector3 size = new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

            Vector3 min = new Vector3(minX, minY, minZ);
            Vector3 max = new Vector3(maxX, maxY, maxZ);

            // Calculate the center based on the center of the verts.
            Vector3 center = min + ((max - min) / 2f);

            Bounds = new Bounds(center, size);
        }

        protected void GenerateBuffers()
        {
            if (m_meshVerts == null || m_meshTriangles == null || VertexCount == 0)
                return;


            if (m_glVbo != -1)
                GL.DeleteBuffer(m_glVbo);
            if (m_glEbo != -1)
                GL.DeleteBuffer(m_glEbo);

            GL.GenBuffers(1, out m_glVbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, m_glVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(m_meshVerts.Length * 4 * 3), m_meshVerts, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out m_glEbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, m_glEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(m_meshTriangles.Length * 4), m_meshTriangles, BufferUsageHint.StaticDraw);
        }
    }
}
