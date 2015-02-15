using OpenTK;

namespace WEditor
{
    /// <summary>
    /// Represents an axis-aligned bounding box.
    /// </summary>
    public class Bounds
    {
        /// <summary> The center of the bounding box. </summary>
        public Vector3 Center;
        /// <summary> Total size of the box. This is always twice the <see cref="Extents"/>. </summary>
        Vector3 Size;
        /// <summary> The extents of the bounding box. This is one half of the size (amount it extends in each direction from center). </summary>
        public Vector3 Extents
        {
            get { return Size / 2f; }
            set { Size = value * 2f; }
        }
        /// <summary> Minimum point on the box. This is always equal to Center - Extents. Read Only.</summary>
        public Vector3 Min
        {
            get { return Center - Extents; }
        }
        /// <summary> Maximum point on the box. This is always equal to Center + Extents. Read Only.</summary>
        public Vector3 Max
        {
            get { return Center + Extents; }
        }
        public Bounds(Vector3 center, Vector3 size)
        {
            Center = center;
            Size = size;
        }

        public override string ToString()
        {
            return string.Format("(center: {0} size: {1} extents: {2} min: {3} max: {4}", Center, Size, Extents, Min, Max);
        }
    }
}
