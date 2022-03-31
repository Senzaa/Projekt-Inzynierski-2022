using OpenTK.Mathematics;
using PISilnik.Base.Physics.Interfaces;
using PISilnik.Rendering;
using PISilnik.Rendering.Interfaces;
using System;

namespace PISilnik.Base.Interfaces
{
    public interface IBasicObject : IRenderable
    {
        int UpdateQueueIndex { get; set; }
        void InitializeOnLoad();
    }
}
