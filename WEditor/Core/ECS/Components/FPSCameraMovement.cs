using OpenTK;
using System;

namespace WEditor
{
    public class FPSCameraMovement : BaseComponent, ILateUpdate
    {
        public float MoveSpeed = 5f;

        public void LateUpdate()
        {
            Vector3 moveDir = Vector3.Zero;
            if(Input.GetKey(System.Windows.Forms.Keys.W))
            {
                moveDir += Vector3.UnitZ;
            }
            if(Input.GetKey(System.Windows.Forms.Keys.S))
            {
                moveDir -= Vector3.UnitZ;
            }
            if(Input.GetKey(System.Windows.Forms.Keys.D))
            {
                moveDir += Vector3.UnitX;
            }
            if(Input.GetKey(System.Windows.Forms.Keys.A))
            {
                moveDir -= Vector3.UnitX;
            }

            if(Input.GetMouseButton(1))
            {
                Rotate(Input.MouseDelta.X, Input.MouseDelta.Y);
            }

            // Early out if we're not moving this frame.
            if (moveDir.LengthFast < 0.1f)
                return;

            float moveSpeed = Input.GetKey(System.Windows.Forms.Keys.ShiftKey) ? MoveSpeed * 2f : MoveSpeed;

            // Normalize the move direction
            moveDir.NormalizeFast();

            // Make it relative to the current rotation.
            moveDir = GetTransform().Rotation.Multiply(moveDir);

            GetTransform().Position += Vector3.Multiply(moveDir, moveSpeed * Time.DeltaTime);

            Console.WriteLine("Position: {0}", GetTransform().Position);
        }

        private void Rotate(float x, float y)
        {
            GetTransform().Rotate(Vector3.UnitY, x * Time.DeltaTime);
            GetTransform().Rotate(GetTransform().Right, y * Time.DeltaTime);

            // Clamp them from looking over the top point.
            Vector3 up = Vector3.Cross(GetTransform().Forward, GetTransform().Right);
            if(Vector3.Dot(up, Vector3.UnitY) < 0.01f)
            {
                GetTransform().Rotate(GetTransform().Right, -y * Time.DeltaTime);
            }
        }
    }
}
