using OpenTK.Graphics.OpenGL;
using System;

namespace WEditor.Rendering
{
    public enum ShaderAttributeIds
    {
        Position = 0,
        Color = 1,
        TexCoord = 2,
    }

    public abstract class BaseRenderer
    {
        public abstract void Initialize();
        public abstract void Render(Camera camera);
        protected void LoadShader(string fileName, ShaderType type, int program, out int address)
        {
            address = GL.CreateShader(type);
            try
            {
                using (var streamReader = new System.IO.StreamReader(fileName))
                {
                    GL.ShaderSource(address, streamReader.ReadToEnd());
                }

                GL.CompileShader(address);
                GL.AttachShader(program, address);

                int compileStatus;
                GL.GetShader(address, ShaderParameter.CompileStatus, out compileStatus);

                if(compileStatus != 1)
                {
                    Console.WriteLine("[WEditor.Core] Failed to compile shader {0}. Log:\n{1}", fileName, GL.GetShaderInfoLog(address));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WEditor.Core] Caught exception while loading shader {0}. Ex: {1}", fileName, ex);
            }
        }
    }
}
