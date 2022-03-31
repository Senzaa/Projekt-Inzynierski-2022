using OpenTK.Mathematics;
using PISilnik.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PISilnik.Rendering.Interfaces
{
    public interface IRenderable : IDisposable
    {
        int RenderQueueIndex { get; set; }
        bool Enabled { get; set; }
        IMesh? Mesh { get; set; }
        Texture[] Textures { get; set; }
        int[] TextureHandles { get; set; }
        Color4 Color { get; set; }
    }
}
