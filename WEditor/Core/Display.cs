using System;

namespace WEditor
{
    public static class Display
    {
        /// <summary> Width of the current viewport display in pixels. Use <see cref="EditorCore.ViewportResize(int, int)"/> to change. Read only. </summary>
        public static int Width { get; private set; }
        /// <summary> Height of the current viewport display in pixels. Use <see cref="EditorCore.ViewportResize(int, int)"/> to change. Read only. </summary>
        public static int Height { get; private set; }

        internal static void Internal_EventResize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}