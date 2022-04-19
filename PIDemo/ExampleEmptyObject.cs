using BepuPhysics;
using BepuPhysics.Collidables;
using OpenTK.Mathematics;
using PISilnik.Base;
using PISilnik.Base.Interfaces;
using PISilnik.Base.Physics;
using PISilnik.Base.Physics.Interfaces;
using PISilnik.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIDemo
{
    public struct ExamplePhysicsObject : IBasicObject, IPhysicsObject
    {
        // IBasicObject
        public int UpdateQueueIndex { get; set; }
        public int RenderQueueIndex { get; set; }

        // IBasicObject -> IRenderable
        public bool Enabled { get; set; }
        public IMesh Mesh { get; set; }
        public Texture[] Textures { get; set; }
        public int[] TextureHandles { get; set; }
        public Color4 Color { get; set; }

        // IPhysicsObject
        public TypedIndex ShapeIndex { get; set; }
        public BodyReference PhysicsBody { get; set; }

        // Przykladowy konstruktor obiektu z cialem fizycznym
        public ExamplePhysicsObject()
        {
            // Wymagane zmienne przez IBasicObject
            RenderQueueIndex = 0;
            UpdateQueueIndex = 0;
            Enabled = true;
            Mesh = new BasicMesh();
            Textures = Array.Empty<Texture>();
            TextureHandles = Array.Empty<int>();
            Color = OpenTK.Mathematics.Color4.White;

            int width = 1,
                height = 1,
                length = 1;
            Box boxShape = new(width, height, length);

            // Rejestracja ksztaltu
            ShapeIndex = Physics.AddShape(boxShape);

            // Parametr inercji obiektu
            BodyInertia bodyInertia = boxShape.ComputeInertia(1);

            // Dodawanie nowego ciala fizycznego do puli cial
            // w bibliotece BEPUphysics
            PhysicsBody = Physics.AddBody(BodyDescription.CreateDynamic(
                new System.Numerics.Vector3(0f),
                bodyInertia,
                new CollidableDescription(
                        shape: ShapeIndex,
                        maximumSpeculativeMargin: 0.01f
                    ),
                new BodyActivityDescription(
                        sleepThreshold: 0.1f
                    )
                ));
        }

        // IBasicObject.InitializeOnLoad
        public void InitializeOnLoad()
        {
            // Ladowanie siatki do pamieci karty graficznej
            Mesh.InitializeGLBindings();
        }
        public void Dispose()
        {

        }
    }
}
