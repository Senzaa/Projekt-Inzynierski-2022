using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Graphics.OpenGL4;
using PISilnik;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using PISilnik.Base.Physics;
using PISilnik.Base.Physics.Interfaces;
using PISilnik.Rendering;
using PISilnik.Rendering.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace PIDemo
{
    public struct SimpleSphere : IBasicObject, IPhysicsObject
    {
        public int RenderQueueIndex { get; set; }
        public int UpdateQueueIndex { get; set; }
        public bool Enabled { get; set; }
        public IMesh Mesh { get; set; }
        public Shader Shader { get; set; }
        public Texture[] Textures { get; set; }
        public int[] TextureHandles { get; set; }
        public TypedIndex ShapeIndex { get; private set; }
        public BodyReference PhysicsBody { get; private set; }
        public OpenTK.Mathematics.Color4 Color { get; set; }

        public SimpleSphere(float radius)
        {
            RenderQueueIndex = 0;
            UpdateQueueIndex = 0;
            Enabled = true;
            Mesh = new BasicMesh();
            Shader = Shader.BasicShader;
            Textures = Array.Empty<Texture>();
            TextureHandles = Array.Empty<int>();
            Color = OpenTK.Mathematics.Color4.White;

            Sphere sphereShape = new(radius);
            ShapeIndex = Physics.AddShape(sphereShape);
            BodyInertia bodyInertia = sphereShape.ComputeInertia(1);

            PhysicsBody = Physics.AddBody(BodyDescription.CreateDynamic(
                new Vector3(0f),
                bodyInertia,
                new CollidableDescription(
                    ShapeIndex,
                    0.01f
                    ),
                new BodyActivityDescription(0.1f)
                ));
        }

        public SimpleSphere() : this(0.5f) { }
        public void InitializeOnLoad()
        {
            Mesh.InitializeGLBindings();
            TextureHandles = Textures.Select(t => t.Handle).ToArray();
        }

        public void Dispose()
            => Engine.ObjectsQueue.DetachObject(ref this);

        public override int GetHashCode()
            => UpdateQueueIndex;
    }
}
