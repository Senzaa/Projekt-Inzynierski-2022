using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PISilnik.Rendering
{
    public class Shader
    {
        public readonly int Handle = -1;

        private readonly Dictionary<string, int> _uniformLocations = new();

        private static readonly char PathSep = Path.DirectorySeparatorChar;
        private static readonly string RootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        public static readonly Shader BasicShader = new(
            $"{RootDir + PathSep}Resources{PathSep}Shaders{PathSep}BasicShader{PathSep}BasicShader.vert",
            $"{RootDir + PathSep}Resources{PathSep}Shaders{PathSep}BasicShader{PathSep}BasicShader.frag"
            );
        public Shader() {
        }
        public Shader(string vertPath, string fragPath, string geomPath = "")
        {
            string shaderSource = File.ReadAllText(vertPath);
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertexShader, shaderSource);

            CompileShader(vertexShader);

            shaderSource = File.ReadAllText(fragPath);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            int geometryShader = -1;
            if (!string.IsNullOrEmpty(geomPath))
            {
                geometryShader = GL.CreateShader(ShaderType.GeometryShader);
                shaderSource = File.ReadAllText(geomPath);
                GL.ShaderSource(geometryShader, shaderSource);
                CompileShader(geometryShader);
            }

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            if (!string.IsNullOrEmpty(geomPath))
                GL.AttachShader(Handle, geometryShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            if (!string.IsNullOrEmpty(geomPath))
                GL.DetachShader(Handle, geometryShader);

            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);
            if (!string.IsNullOrEmpty(geomPath))
                GL.DeleteShader(geometryShader);
            
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                string key = GL.GetActiveUniform(Handle, i, out _, out _);
                int location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);
            }
        }
        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
                throw new Exception($"Error occurred whilst compiling Shader({shader}):{Environment.NewLine}{GL.GetShaderInfoLog(shader)}");
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
                throw new Exception($"Error occurred whilst linking Program({program}:{Environment.NewLine}{GL.GetProgramInfoLog(program)})");
        }
        public void Use()
            => GL.UseProgram(Handle);
        public int GetAttribLocation(string attribName)
            => GL.GetAttribLocation(Handle, attribName);
        public void SetInt(string name, int data)
            => GL.Uniform1(_uniformLocations[name], data);
        public void SetFloat(string name, float data)
            => GL.Uniform1(_uniformLocations[name], data);
        public void SetMatrix4(string name, Matrix4 data)
            => GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        public void SetVector3(string name, Vector3 data)
            => GL.Uniform3(_uniformLocations[name], data);
        public void SetVector4(string name, Vector4 data)
            => GL.Uniform4(_uniformLocations[name], data);
        public void SetColor4(string name, Color4 data)
            => GL.Uniform4(_uniformLocations[name], data);
    }
}
