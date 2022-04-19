using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using BepuUtilities.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PISilnik.Base.Physics
{
    public static class Physics
    {
        public static Vector3 Gravity { get; set; } = new(0, 0, 0);
        public static bool Initialized { get; set; } = false;
        public static bool Running { get; set; } = true;

        private static Simulation? SimulationValue { get; set; }
        internal static Simulation Simulation
        {
            get
            {
                if (Initialized)
                    return SimulationValue!;
                else
                    throw new Exception("Physics engine is not yet initialized");
            }
            set => SimulationValue = value;
        }
        private static BufferPool? bufferPool = null;
        internal static BufferPool BufferPool { get {
                if (Initialized)
                    return bufferPool!;
                else
                    throw new Exception("Physics engine is not yet initialized");
            } set => bufferPool = value; }
        private static ThreadDispatcher? ThreadDispatcher { get; set; }
        internal static void Initialize()
        {
            if (!Initialized)
            {
                try
                {
                    Initialized = true;
                    BufferPool = new();
                    Simulation = Simulation.Create(BufferPool,
                        new DefaultNarrowPhaseCallbacks(
                            new(60f, 10f),
                            frictionCoefficient: 1f
                            ),
                        new DefaultPoseIntegratorCallbacks(),
                        new SolveDescription(8, 1)
                        );
                    ThreadDispatcher = new(Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1));
                    //ThreadDispatcher = new(8);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to initialize physics engine:{Environment.NewLine}{ex}");
                }
            }
            else
                throw new Exception("Physics engine is already initialized");
        }

        private static float timestepDuration = 1 / 144f;
        public static void SetSpeed(float newTimestepDuration)
        {
            if (newTimestepDuration > 0f && newTimestepDuration < 1f)
                timestepDuration = newTimestepDuration;
            else
                Console.WriteLine("Timestep is out of bounds!");
        }
        public static float GetSpeed() => timestepDuration;
        public static void Timestep() {
            if (Running)
                Simulation.Timestep(timestepDuration, ThreadDispatcher);
        }
        public static TypedIndex AddShape<T>(T shape) where T : unmanaged, IShape
            => Simulation.Shapes.Add(shape);
        public static ref TShape GetShape<TShape>(TypedIndex shapeIndex) where TShape : unmanaged, IShape
            => ref Simulation.Shapes.GetShape<TShape>(shapeIndex.Index);
        public static BodyReference AddBody(BodyDescription bodyDescription)
            => Simulation.Bodies[Simulation.Bodies.Add(bodyDescription)];
        public static StaticReference AddStatic(StaticDescription staticDescription)
            => Simulation.Statics.GetStaticReference(Simulation.Statics.Add(staticDescription));
    }
}
