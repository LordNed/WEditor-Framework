using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WEditor.Rendering
{
    public class Mesh
    {
        /// <summary> The bounding volume of the mesh. This is an axis-aligned bounding box in local space. Setting Verticies will automatically update the Bounds. (Read Only) </summary>
        public Bounds Bounds { get; private set; }
        /// <summary> Vertex colors of the mesh. </summary>
        public Color[] Colors;
        /// <summary> Vertex normals of the mesh. </summary>
        public Vector3[] Normals;
        /// <summary> Triangle indexes of the mesh. </summary>
        public int[] Triangles;
        /// <summary> Vertex positions of the mesh. </summary>
        public Vector3[] Verticies;
        /// <summary> Texture Coordinates of the mesh. </summary>
        public Vector2[] UVs;
        /// <summary> Vertex count of the mesh. (Read Only) </summary>
        public int VertexCount { get; private set; }
        /// <summary> MeshTopology lets you render complex things using lines or points instead of just triangles if desired. </summary>
        //public PrimitiveType MeshTopology;

        public Mesh()
        {
            Bounds = new Bounds(Vector3.Zero, Vector3.Zero);
            Colors = null;
            Normals = null;
            Triangles = null;
            Verticies = null;
            UVs = null;
            VertexCount = 0;
            //MeshTopology = PrimitiveType.Triangles;
        }
    }
}
