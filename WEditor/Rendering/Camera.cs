using OpenTK;
using System.Drawing;

namespace WEditor.Rendering
{
    public class Camera : Transform
    {
        /// <summary> The near clipping plane distance. </summary>
        public float NearClipPlane = 250f;
        /// <summary> The far clipping plane distance. </summary>
        public float FarClipPlane = 45000f;
        /// <summary> Vertical field of view in degrees. </summary>
        public float FieldOfView = 45f;
        /// <summary> Viewport width/height. Read only. </summary>
        public float AspectRatio { get { return PixelWidth / (float)PixelHeight; } }
        /// <summary> Width of the camera viewport in pixels. Read only. </summary>
        public int PixelWidth { get { return (int)(PixelRect.Width); } }
        /// <summary> Height of the camera viewport in pixels. Read only. </summary>
        public int PixelHeight { get { return (int)(PixelRect.Height); } }
        /// <summary> Color to clear the backbuffer with. </summary>
        public Color ClearColor;
        /// <summary> Where on screen the camera is rendered (in normalized coordinates) </summary>
        public Rect Rect
        {
            get
            {
                return m_rect;
            }
            set
            {
                m_rect.Width = MathE.Clamp(value.Width, 0, 1);
                m_rect.Height = MathE.Clamp(value.Height, 0, 1);
                m_rect.X = MathE.Clamp(value.X, 0, 1);
                m_rect.Y = MathE.Clamp(value.Y, 0, 1);

                //Update the Projection matrix.
                ProjMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FieldOfView), AspectRatio, NearClipPlane, FarClipPlane);
            }
        }
        /// <summary> Where on screen the camera is rendered (in normalized coordinates) </summary>
        public Rect PixelRect
        {
            get
            {
                Rect pixelRect = new Rect();
                pixelRect.X = m_rect.X * Display.Width;
                pixelRect.Y = m_rect.Y * Display.Height;
                pixelRect.Width = m_rect.Width * Display.Width;
                pixelRect.Height = m_rect.Height * Display.Height;

                return pixelRect;
            }
            set
            {
                // Update the normalized Viewport rect to make setting PixelRect and Rect interchangeable.
                Rect = new Rect(value.X / Display.Width, value.Y / Display.Height, value.Width / Display.Width, value.Height / Display.Height);
            }
        }

        /// <summary> View matrix. Calculated each time <see cref="ViewMatrix"/> is called as there's no way for a Transform to mark itself as dirty yet. </summary>
        public Matrix4 ViewMatrix
        {
            get
            {
                return Matrix4.LookAt(Position, Position + Forward, Vector3.UnitY);
            }
        }

        /// <summary> Projection matrix. Calculated when the onscreen <see cref="Rect"/> gets modified. </summary>
        public Matrix4 ProjMatrix { get; private set; }

        /// <summary> Backing field for normalized viewport rect. </summary>
        private Rect m_rect;

        public Camera()
        {
            Rect = new Rect(0f, 0f, 1f, 1f);
            ClearColor = Color.ForestGreen;
            EditorCore.Instance.RegisterCamera(this);
        }

        public Ray ViewportPointToRay(Vector3 mousePos)
        {
            Vector3 mousePosA = new Vector3(mousePos.X, mousePos.Y, -1f);
            Vector3 mousePosB = new Vector3(mousePos.X, mousePos.Y, 1f);


            Vector4 nearUnproj = UnProject(ProjMatrix, ViewMatrix, mousePosA);
            Vector4 farUnproj = UnProject(ProjMatrix, ViewMatrix, mousePosB);

            Vector3 dir = farUnproj.Xyz - nearUnproj.Xyz;
            dir.Normalize();

            return new Ray(nearUnproj.Xyz, dir);
        }

        public Vector4 UnProject(Matrix4 projection, Matrix4 view, Vector3 mouse)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / PixelWidth - 1;
            vec.Y = -(2.0f * mouse.Y / PixelHeight - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > float.Epsilon || vec.W < float.Epsilon)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec;
        }
    }
}
