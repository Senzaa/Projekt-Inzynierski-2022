using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PISilnik;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using PISilnik.Base.Physics;
using PISilnik.Base.Physics.Interfaces;
using PISilnik.Rendering;
using PISilnik.Rendering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Vector3 = System.Numerics.Vector3;

namespace PIDemo
{
    public class SimpleObjectsMaster : IObjectsMaster
    {

        private readonly OpenTK.Mathematics.Vector3[] _pointLightPositions =
        {
            new(0.7f, 0.2f, 2.0f),
            new(2.3f, -3.3f, -4.0f),
            new(-4.0f, 2.0f, -12.0f),
            new(0.0f, 0.0f, -3.0f)
        };
        public void Render(IEnumerable<IRenderable> basicObjects, double deltaTime)
        {
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            Shader.BasicShader.Use();
            Shader.BasicShader.SetMatrix4("view", Engine.MainCamera.GetViewMatrix());
            Shader.BasicShader.SetMatrix4("projection", Engine.MainCamera.GetProjectionMatrix());
            
            Shader.BasicShader.SetInt("materialDiffuse", 0);
            Shader.BasicShader.SetInt("materialNormal", 1);
            Shader.BasicShader.SetInt("materialSpecular", 2);

            Shader.BasicShader.SetVector3("viewPos", Engine.MainCamera.Position);
            Shader.BasicShader.SetVector3("sunDir", new(-0.5f, -1.0f, 0.5f));
            Shader.BasicShader.SetVector3("sunAmbient", new(1.0f, 1.0f, 1.0f));

            for (int i = 0; i < _pointLightPositions.Length; i++)
            {
                Shader.BasicShader.SetInt($"pointLights[{i}].enabled", 0);
            }

            {
                GroundPlane ground = basicObjects.OfType<GroundPlane>().FirstOrDefault();
                Box shape = Physics.GetShape<Box>(ground.ShapeIndex);
                Shader.BasicShader.SetMatrix4("model",
                    Matrix4.Identity *
                    Matrix4.CreateScale(
                        shape.HalfWidth,
                        shape.HalfHeight,
                        shape.HalfLength
                    ) *
                    Matrix4.CreateTranslation(
                        ground.StaticBody.Pose.Position.X,
                        ground.StaticBody.Pose.Position.Y,
                        ground.StaticBody.Pose.Position.Z
                    ));
                Shader.BasicShader.SetFloat("shininess", 32f);
                ground.Mesh.Use();

                GL.BindTextures(0, ground.Textures.Length, ground.TextureHandles);
                GL.DrawArrays(PrimitiveType.Triangles, 0, ground.Mesh.ElementVerticesCount);
            }

            foreach (SimpleCube basicObject in basicObjects.OfType<SimpleCube>())
            {
                Box shape = Physics.GetShape<Box>(basicObject.ShapeIndex);
                Shader.BasicShader.SetMatrix4("model",
                    Matrix4.Identity *
                    Matrix4.CreateScale(
                        shape.HalfWidth,
                        shape.HalfHeight,
                        shape.HalfLength
                    ) *
                    Matrix4.CreateFromQuaternion(
                    new(
                        basicObject.PhysicsBody.Pose.Orientation.X,
                        basicObject.PhysicsBody.Pose.Orientation.Y,
                        basicObject.PhysicsBody.Pose.Orientation.Z,
                        basicObject.PhysicsBody.Pose.Orientation.W
                        )
                    ) *
                    Matrix4.CreateTranslation(
                        basicObject.PhysicsBody.Pose.Position.X,
                        basicObject.PhysicsBody.Pose.Position.Y,
                        basicObject.PhysicsBody.Pose.Position.Z
                    )
                    );
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    throw new Exception("Linux Test 2:" + Environment.NewLine +
                        $"Mesh Name: {basicObject.Mesh.Name}" + Environment.NewLine +
                        $"Mesh EVC: {basicObject.Mesh.ElementVerticesCount}" + Environment.NewLine +
                        $"Mesh Polygon Groups: {basicObject.Mesh.PolygonGroups.Count}" + Environment.NewLine
                        );
                }
                Shader.BasicShader.SetFloat("shininess", basicObject.Mesh.Materials["Material"].Shininess/10);
                basicObject.Mesh.Use();

                GL.BindTextures(0, basicObject.Textures.Length, basicObject.TextureHandles);

                GL.DrawArrays(PrimitiveType.Triangles, 0, basicObject.Mesh.ElementVerticesCount);
            }

            foreach (SimpleSphere basicObject in basicObjects.OfType<SimpleSphere>())
            {
                Sphere shape = Physics.GetShape<Sphere>(basicObject.ShapeIndex);
                Shader.BasicShader.SetMatrix4("model",
                    Matrix4.Identity *
                    Matrix4.CreateScale(
                        shape.Radius,
                        shape.Radius,
                        shape.Radius
                    ) *
                    Matrix4.CreateFromQuaternion(
                    new(
                        basicObject.PhysicsBody.Pose.Orientation.X,
                        basicObject.PhysicsBody.Pose.Orientation.Y,
                        basicObject.PhysicsBody.Pose.Orientation.Z,
                        basicObject.PhysicsBody.Pose.Orientation.W
                        )
                    ) *
                    Matrix4.CreateTranslation(
                        basicObject.PhysicsBody.Pose.Position.X,
                        basicObject.PhysicsBody.Pose.Position.Y,
                        basicObject.PhysicsBody.Pose.Position.Z
                    )
                    );
                Shader.BasicShader.SetFloat("shininess", 32);
                basicObject.Mesh.Use();

                GL.BindTextures(0, basicObject.Textures.Length, basicObject.TextureHandles);

                GL.DrawArrays(PrimitiveType.Triangles, 0, basicObject.Mesh.ElementVerticesCount);
            }
        }

        readonly Random random = new();
        private readonly Dictionary<int, RigidPose> quickScreenshot = new();
        private readonly float[] speeds = new float[] {
            1f / 120f,
            1f / 240f,
            1f / 500f,
            1f / 1000f,
            1f / 2500f,
            1f / 10000f,
            1f / 20000f
        };
        private int speed = 0;
        public void Update(IEnumerable<IBasicObject> basicObjects, double deltaTime)
        {
            KeyboardState keyboardState = Engine.GameWindow.KeyboardState;

            if (keyboardState.IsKeyReleased(Keys.F1))
            {
                quickScreenshot.Clear();

                foreach (IPhysicsObject basicObject in basicObjects.OfType<IPhysicsObject>())
                    quickScreenshot.Add(basicObject.UpdateQueueIndex, basicObject.PhysicsBody.Pose);

            } else if (keyboardState.IsKeyReleased(Keys.F2))
            {
                IPhysicsObject[] physicsObjects = basicObjects.OfType<IPhysicsObject>().ToArray();
                foreach (KeyValuePair<int, RigidPose> kvp in quickScreenshot)
                {
                    IPhysicsObject physicsObject = physicsObjects.FirstOrDefault(bo => bo.UpdateQueueIndex == kvp.Key);
                    if (physicsObject != null)
                    {
                        BodyReference br = physicsObject.PhysicsBody;
                        br.Pose.Position = kvp.Value.Position;
                        br.Pose.Orientation = kvp.Value.Orientation;
                        br.Velocity.Linear = Vector3.Zero;
                        br.Velocity.Angular = Vector3.Zero;
                        br.Awake = true;
                    }
                }
            }

            if (keyboardState.IsKeyReleased(Keys.F) ||
                keyboardState.IsKeyReleased(Keys.T))
            {
                SimpleCube cube = new();
                cube.PhysicsBody.Pose.Position = new Vector3(
                        Engine.MainCamera.Position.X,
                        Engine.MainCamera.Position.Y,
                        Engine.MainCamera.Position.Z
                    );

                if (keyboardState.IsKeyReleased(Keys.F))
                {
                    OpenTK.Mathematics.Vector3 lookVector = Engine.MainCamera.Front.Normalized();
                    cube.PhysicsBody.Velocity.Linear = new Vector3(
                        lookVector.X,
                        lookVector.Y,
                        lookVector.Z
                    ) * 50f;

                    cube.PhysicsBody.ApplyAngularImpulse(new Vector3(
                            random.NextSingle() * 4f - 2f,
                            random.NextSingle() * 4f - 2f,
                            random.NextSingle() * 4f - 2f
                        ));
                }

                cube.Mesh = Program.MeshBank["SimpleCube"];
                cube.Textures = Program.CubeTextures;

                Engine.ObjectsQueue.AttachObject(ref cube);
            }

            if (keyboardState.IsKeyReleased(Keys.Y) ||
                keyboardState.IsKeyReleased(Keys.U))
            {
                SimpleSphere sphere = new();
                sphere.PhysicsBody.Pose.Position = new Vector3(
                        Engine.MainCamera.Position.X,
                        Engine.MainCamera.Position.Y,
                        Engine.MainCamera.Position.Z
                    );

                if (keyboardState.IsKeyReleased(Keys.U))
                {
                    OpenTK.Mathematics.Vector3 lookVector = Engine.MainCamera.Front.Normalized();
                    sphere.PhysicsBody.Velocity.Linear = new Vector3(
                        lookVector.X,
                        lookVector.Y,
                        lookVector.Z
                    ) * 50f;

                    sphere.PhysicsBody.ApplyAngularImpulse(new Vector3(
                            random.NextSingle() * 4f - 2f,
                            random.NextSingle() * 4f - 2f,
                            random.NextSingle() * 4f - 2f
                        ));
                }

                sphere.Mesh = Program.MeshBank["SimpleSphere"];
                sphere.Textures = Program.SphereTextures;

                Engine.ObjectsQueue.AttachObject(ref sphere);
            }

            if (keyboardState.IsKeyReleased(Keys.G))
            {
                Physics.Gravity = Physics.Gravity.Y != 0f ? new(0f) : new(0f, -9.7f, 0f);
                foreach (IPhysicsObject basicObject in basicObjects.OfType<IPhysicsObject>())
                {
                    BodyReference bodyReference = basicObject.PhysicsBody;
                    bodyReference.Awake = true;
                }
            }

            if (keyboardState.IsKeyReleased(Keys.PageUp))
            {
                if (speed > 0)
                    speed--;
            } else if (keyboardState.IsKeyReleased(Keys.PageDown))
            {
                if (speed < speeds.Length - 1)
                    speed++;
            }

            if (keyboardState.IsKeyReleased(Keys.X))
            {
                quickScreenshot.Clear();
                int count = Engine.ObjectsQueue.UpdateQueue.Count - 1;
                for (int i = 0; i < count; i++)
                    Engine.ObjectsQueue.DetachObjectAt(1);
            }

            if (keyboardState.IsKeyReleased(Keys.V))
            {
                Physics.Running = !Physics.Running;
            } else if (keyboardState.IsKeyReleased(Keys.B))
            {
                Physics.Timestep();
            }

            if (keyboardState.IsKeyDown(Keys.Apostrophe))
                Console.WriteLine($"Current FPS is {1/deltaTime}");

            Physics.SetSpeed(speeds[speed]);
        }
    }
}
