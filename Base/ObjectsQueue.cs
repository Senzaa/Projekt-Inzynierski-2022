using PISilnik.Base.Interfaces;
using PISilnik.Rendering.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PISilnik.Base
{
    public class ObjectsQueue
    {
        public List<IBasicObject> RenderQueue { get; private set; } = new();
        public List<IBasicObject> UpdateQueue { get; private set; } = new();

        public void AttachObject<T>(
            ref T basicObject
            ) where T : IBasicObject, IRenderable
        {
            if (Engine.Initialized)
                basicObject.InitializeOnLoad();
            basicObject.RenderQueueIndex = RenderQueue.Count;
            basicObject.UpdateQueueIndex = UpdateQueue.Count;
            RenderQueue.Add(basicObject);
            UpdateQueue.Add(RenderQueue[basicObject.RenderQueueIndex]);
        }

        public void DetachObject<T>(ref T basicObject) where T : IBasicObject
            => DetachObjectAt(basicObject.UpdateQueueIndex);

        public void DetachObjectAt(int index)
        {
            IBasicObject basicObject = UpdateQueue[index];
            UpdateQueue.RemoveAt(basicObject.UpdateQueueIndex);
            RenderQueue.RemoveAt(basicObject.RenderQueueIndex);
            for (int i = 0; i < UpdateQueue.Count; i++)
                UpdateQueue[i].UpdateQueueIndex = i;
            for (int i = 0; i < RenderQueue.Count; i++)
                RenderQueue[i].RenderQueueIndex = i;
        }

        public void InitializeBasicObjects()
        {
            for (int i = 0; i < UpdateQueue.Count; i++)
            {
                IBasicObject basicObject = UpdateQueue[i];
                basicObject.InitializeOnLoad();
                UpdateQueue[basicObject.UpdateQueueIndex] = basicObject;
                RenderQueue[basicObject.RenderQueueIndex] = basicObject;
            }
        }
    }
}
