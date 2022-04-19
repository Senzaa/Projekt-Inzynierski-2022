using PISilnik;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using PISilnik.Base.Physics;
using PISilnik.Helper;
using PISilnik.Rendering;
using SixLabors.ImageSharp.ColorSpaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PIDemo
{
    public class Program
    {
        //[DllImport("kernel32")]
        //static extern bool AllocConsole();

        private static readonly char PathSep = Path.DirectorySeparatorChar;
        private static readonly string RootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        public static Texture[] CubeTextures { get; private set; }
        public static Texture[] SphereTextures { get; private set; }

        public static readonly Dictionary<string, BasicMesh> MeshBank = new();

        static void Main(string[] args)
        {
            //AllocConsole();
            Console.WriteLine("Initializing Demo Window...");

            //Physics.Gravity = new(0f, -9.7f, 0);

            Engine.Initialize(
                    OpenTK.Windowing.Desktop.GameWindowSettings.Default,
                    new OpenTK.Windowing.Desktop.NativeWindowSettings()
                    {
                        Title = "PI Demo",
#if DEBUG
                        Flags = OpenTK.Windowing.Common.ContextFlags.Debug,
#endif
                        WindowState = OpenTK.Windowing.Common.WindowState.Maximized
                    }
                );

            Engine.GameWindow.Load += () =>
            {
                try
                {
                    MeshBank.Add("SimpleCube", new WavefrontOBJ($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Meshes{PathSep}SimpleCube.obj").Meshes[0]);
                    MeshBank.Add("SimpleSphere", new WavefrontOBJ($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Meshes{PathSep}SimpleSphere.obj").Meshes[0]);

                    CubeTextures =
                        new Texture[3]
                        {
                            Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}SimpleCube_Diffuse.png"),
                            Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}SimpleCube_Normal.png"),
                            Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}SimpleCube_Specular.png")
                        };

                    SphereTextures =
                        new Texture[]
                        {
                            Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}SimpleSphere_Diffuse.png"),
                            Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}SimpleSphere_Normal.png"),
                            Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}SimpleSphere_Specular.png")
                        };

                    Engine.MainCamera.Fov = 70;
                    Engine.MainCamera.RenderDistance = 1000f;
                    Engine.MainCamera.Position = new(0f, 10f, 0f);

                    GroundPlane ground = new(500f, 1f, 500f);
                    {
                        ground.Mesh = MeshBank["SimpleCube"].Clone();
                        ground.Mesh.Name = "GroundMesh";

                        Texture gpTexDiffuse = Texture.Empty(512, 512); //Texture.LoadFromFile($"{RootDir + PathSep}Resources{PathSep}Builtin{PathSep}Textures{PathSep}Grass.jpg");
                        Texture gpTexSpecular = Texture.Empty(512, 512);

                        float[] perlinMap = Texture.GeneratePerlinMap(
                                                gpTexDiffuse.Width,
                                                gpTexDiffuse.Height,
                                                scale: 100f,
                                                zSlice: 0f);

                        SixLabors.ImageSharp.ColorSpaces.Conversion.ColorSpaceConverter csc = new();

                        gpTexDiffuse.ApplyMask(
                            perlinMap,
                            (noise, pixel) =>
                            {
                                float S = 1f;
                                float V = noise;
                                Rgb pixelRGB = csc.ToRgb(
                                        new Hsv(172f, S, V)
                                    );
                                return new(
                                        pixelRGB.R,
                                        pixelRGB.G,
                                        pixelRGB.B,
                                        pixel.A
                                    );
                            },
                            intensity: 0.5f
                        );

                        gpTexSpecular.ApplyMask(
                            perlinMap,
                            (noise, pixel) =>
                            {
                                return new(
                                        noise,
                                        noise,
                                        noise,
                                        pixel.A
                                    );
                            },
                            intensity: 1.0f
                        );

                        ground.Textures = new Texture[] {
                            gpTexDiffuse,                            // diffuse
                            Texture.Empty(1, 1, 255, 255, 255, 255), // normal
                            gpTexSpecular                            // specular
                        };
                    }

                    Engine.ObjectsQueue.AttachObject(ref ground);

                    Random random = new();

                    Physics.Running = false;

                    const int sizeX = 50;
                    const int sizeY = 50;
                    for (int iy = 0; iy < sizeY; iy++)
                    {
                        for (int ix = 0; ix < sizeX; ix++)
                        {
                            for (int iz = 0; iz < 1; iz++)
                            {
                                SimpleCube cube = new();

                                cube.Mesh = MeshBank["SimpleCube"];
                                cube.Textures = CubeTextures;
                                cube.PhysicsBody.Pose.Position = new Vector3(
                                    ix - (sizeX / 2f),
                                    iy,
                                    iz + 20f
                                    );

                                Engine.ObjectsQueue.AttachObject(ref cube);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            };

            try
            {
                Engine.Run();
            } catch (Exception ex)
            {
                Console.WriteLine($"An error occured during runtime:{Environment.NewLine}{ex}");
            }

            Console.WriteLine("Exiting Demo Application");
        }
    }
}
