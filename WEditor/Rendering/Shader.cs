using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor.Rendering
{
    public class Shader : IDisposable
    {
        public void Use()
        {
            GL.UseProgram(ProgramId);
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }

        public string Name;
        public int ProgramId;
        public int UniformMVP;
    }
}
