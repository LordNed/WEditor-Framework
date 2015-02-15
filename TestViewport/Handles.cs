using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using OpenTK;
using WindViewer.Editor;

namespace TestViewport
{
    public class Handles
    {
        public static Matrix4 matrix
        {
            get { return s_Matrix; }
            set
            {
                s_Matrix = value;
                s_InverseMatrix = value.Inverted();
            }
        }

        public static Matrix4 inverseMatrix
        {
            get { return s_InverseMatrix; }
        }

        internal static Matrix4 s_Matrix = Matrix4.Identity;
        internal static Matrix4 s_InverseMatrix = Matrix4.Identity;


        public static Vector3 DoPositionHandle(Vector3 position)
        {
            // Compute the new position of the handle as 3 individual 1D movement checks.

            // Get a unique ID for this handle.
            // Each call to Slider needs to have a unique id... but meh. 


            position = Handles.Slider(1, position, -Vector3.UnitX);
            position = Handles.Slider(2, position, Vector3.UnitY);
            //position = Handles.Slider(position, Vector3.UnitZ);


            // Then do Planar movements.
            // ...

            return position;
        }


        public static Vector3 Slider(int id, Vector3 position, Vector3 direction)
        {
            // Get a unique ID for this handle.
            // Each call to Slider needs to have a unique id... but meh. 
            return Slider1D.Do(id, position, direction, direction);
        }
    }

    public static class GUIUtility
    {
        public static int HotControl;

        public static bool ControlIsSelected()
        {
            return HotControl != 0;
        }
    }

    public class Slider1D
    {
        private static Vector2 s_StartMousePosition; // Mouse position (screen space)
        private static Vector2 s_CurrentMousePosition; // Mouse position (screen space)
        private static Vector3 s_StartPosition; // Position (worldspace) slider starts at.

        public static Vector3 Do(int id, Vector3 position, Vector3 handleDirection, Vector3 slideDirection)
        {
            Event current = Event.current;
            switch (current.Type) //current.GetTypeForControl(id)
            {
                case EventType.MouseDown:
                    if ((HandleUtility.IsNearestControl(id) && current.button == 0) && !GUIUtility.ControlIsSelected())
                    {
                        // Set selected control to this one.
                        GUIUtility.HotControl = id;
                        Slider1D.s_CurrentMousePosition = Slider1D.s_StartMousePosition = current.mousePosition.Xy;
                        Slider1D.s_StartPosition = position;
                        current.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if ((GUIUtility.HotControl == id && current.button == 0))
                    {
                        // Set selected control to none.
                        GUIUtility.HotControl = 0;
                        current.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.HotControl == id)
                    {
                        Slider1D.s_CurrentMousePosition += current.delta;
                        float slideDistance = CalcLineTranslation(Slider1D.s_StartMousePosition,
                            Slider1D.s_CurrentMousePosition, Slider1D.s_StartPosition, slideDirection);

                        //Vector3 vector3 = Handles.matrix.MultiplyVector(slideDirection);
                        //Vector3 v = Handles.s_Matrix.MultiplyPoint(Slider1D.s_StartPosition) + vector3*slideDistance;
                        //position = Handles.s_InverseMatrix.MultiplyPoint(v);

                        Vector3 vector3 = Vector3.TransformVector(slideDirection, Handles.matrix);
                        Vector3 v = Vector3.TransformPosition(Slider1D.s_StartPosition, Handles.matrix) +
                                    vector3 * slideDistance;
                        position = Vector3.TransformPosition(v, Handles.s_InverseMatrix);


                        current.Use();
                    }
                    break;
                case EventType.Layout:
                    // In the layout pass, we're going to register things to be hit and stuff.
                    HandleUtility.AddControl(id, HandleUtility.DistanceToLine(position, position + slideDirection * 1f /* size */));
                    HandleUtility.AddControl(id, HandleUtility.DistanceToCircle(position + slideDirection * 1f /* size */, 1f * 0.2f /* size */));
                    break;
            }

            return position;
        }

        public static float CalcLineTranslation(Vector2 src, Vector2 dest, Vector3 srcPosition, Vector3 constraintDir)
        {
            //srcPosition = Handles.matrix.MultiplyPoint(srcPosition);
            //constraintDir = Handles.matrix.MultiplyVector(constraintDir);
            srcPosition = Vector3.TransformPosition(srcPosition, Handles.matrix);
            constraintDir = Vector3.TransformVector(constraintDir, Handles.matrix);

            Vector3 forward = Camera.Current.transform.Forward;

            float direction = 1f;
            if (Vector3.Dot(constraintDir, forward) < 0.0)
                direction = -1f;

            Vector3 vector3 = constraintDir;
            vector3.Y = -vector3.Y;

            Camera current = Camera.Current;
            Vector2 x1 = current.WorldToScreenPoint(srcPosition);
            Vector2 x2 = current.WorldToScreenPoint(srcPosition + constraintDir * direction);
            Vector2 x0_1 = dest;
            Vector2 x0_2 = src;

            if (x1 == x2)
                return 0f;

            x0_1.Y = -x0_1.Y;
            x0_2.Y = -x0_2.Y;

            float parametrization = GetParametrization(x0_2, x1, x2);
            return (GetParametrization(x0_1, x1, x2) - parametrization) * direction;
        }

        public static float GetParametrization(Vector2 x0, Vector2 x1, Vector2 x2)
        {
            return -(Vector2.Dot(x1 - x0, x2 - x1) / (x2 - x1).LengthSquared);
        }
    }

    public class HandleUtility
    {
        public static float s_NearestDistance;
        public static int s_NearestControl;

        public static void AddControl(int controlId, float distance)
        {
            if (distance > HandleUtility.s_NearestDistance)
                return;

            HandleUtility.s_NearestDistance = distance;
            HandleUtility.s_NearestControl = controlId;
        }

        public static float DistanceToLine(Vector3 p1, Vector3 p2)
        {
            p1 = new Vector3(HandleUtility.WorldToGUIPoint(p1));
            p2 = new Vector3(HandleUtility.WorldToGUIPoint(p2));

            float distance = HandleUtility.DistancePointLine(Event.current.mousePosition, p1, p2);
            if (distance < 0f)
                distance = 0f;

            return distance;
        }

        public static float DistanceToCircle(Vector3 position, float radius)
        {
            Vector2 vector2_1 = WorldToGUIPoint(position);
            Camera current = Camera.Current;
            if (current != null)
            {
                Vector2 vector2_2 = HandleUtility.WorldToGUIPoint(position + current.transform.Right * radius);
                radius = (vector2_1 - vector2_2).Length;
            }

            float magnitude = (vector2_1 - Event.current.mousePosition.Xy).Length;
            if (magnitude < radius)
                return 0f;

            return magnitude - radius;
        }

        public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return (HandleUtility.ProjectPointLine(point, lineStart, lineEnd) - point).Length;
        }

        public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector3 = lineEnd - lineStart;
            float magnittude = vector3.Length;
            Vector3 lhs = vector3;
            if (magnittude > 0f)
                lhs /= magnittude;

            float num = MathExtensions.Clamp(Vector3.Dot(lhs, rhs), 0.0f, magnittude);
            return lineStart + lhs * num;
        }

        public static Vector2 WorldToGUIPoint(Vector3 worldPos)
        {
            worldPos = Vector3.TransformPosition(worldPos, Handles.matrix);
            Camera current = Camera.Current;
            if (current == null)
                return new Vector2(worldPos.X, worldPos.Y);

            Vector2 absolutePos = (Vector2)current.WorldToScreenPoint(worldPos);
            absolutePos.Y = (float)Display.Height - absolutePos.Y;

            return HandleUtility.Clip(absolutePos);
        }

        private static Vector2 Clip(Vector2 screenPos)
        {
            screenPos.X = MathExtensions.Clamp(screenPos.X, 0, Display.Width);
            screenPos.Y = MathExtensions.Clamp(screenPos.Y, 0, Display.Height);
            return screenPos;
        }

        public static bool IsNearestControl(int id)
        {
            return s_NearestDistance < 120f && id == s_NearestControl;
        }
    }
}