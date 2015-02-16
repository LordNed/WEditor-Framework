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

        public virtual void PostRenderUpdate() { }

        public abstract void Render(Camera camera);

        protected virtual Shader CreateShaderFromFile(string name, string vertShaderPath, string fragShaderPath)
        {
            Shader shader = new Shader();
            shader.Name = name;

            // Initialize an OpenGL program for the shader.
            shader.ProgramId = GL.CreateProgram();

            int vertShaderId, fragShaderId;
            LoadShader(vertShaderPath, ShaderType.VertexShader, shader.ProgramId, out vertShaderId);
            LoadShader(fragShaderPath, ShaderType.FragmentShader, shader.ProgramId, out fragShaderId);

            // Once the shaders are created and linked to the program we can technically delete the shader.
            // This de-increments the internal reference count so that only the Program is holding onto it.
            // That way, once we delete the Program, it'll destroy itself.
            GL.DeleteShader(vertShaderId);
            GL.DeleteShader(fragShaderId);

            GL.BindAttribLocation(shader.ProgramId, (int)ShaderAttributeIds.Position, "vertexPos");

            // Link the program now that we've compiled it.
            GL.LinkProgram(shader.ProgramId);

            if (GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[WEditor.Core] Error linking shader. Result: {0}", GL.GetProgramInfoLog(shader.ProgramId));
                return null;
            }

            shader.UniformMVP = GL.GetUniformLocation(shader.ProgramId, "modelview");
            shader.UniformColor = GL.GetUniformLocation(shader.ProgramId, "inColor");


            if (GL.GetError() != ErrorCode.NoError)
            {
                Console.WriteLine("[WEditor.Core] Failed to get modelview uniform for shader. Result: {0}", GL.GetProgramInfoLog(shader.ProgramId));
                return null;
            }

            return shader;
        }

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
