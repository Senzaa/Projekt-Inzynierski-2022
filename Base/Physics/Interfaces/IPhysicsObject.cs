using BepuPhysics;
using BepuPhysics.Collidables;
using PISilnik.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PISilnik.Base.Physics.Interfaces
{
    public interface IPhysicsObject : IBasicObject
    {
        TypedIndex ShapeIndex { get; }
        BodyReference PhysicsBody { get; }
    }
    public interface IStaticPhysicsObject : IBasicObject
    {
        TypedIndex ShapeIndex { get; }
        StaticReference StaticBody { get; }
    }
}
