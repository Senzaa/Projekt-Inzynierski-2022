using PISilnik.Base.Interfaces;
using PISilnik.Rendering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PISilnik.Base
{
    public interface IObjectsMaster
    {
        void Render(IEnumerable<IRenderable> basicObjects, double deltaTime);
        void Update(IEnumerable<IBasicObject> basicObjects, double deltaTime);
    }
}
