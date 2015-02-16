using System;
using System.Collections.Generic;

namespace WEditor.Rendering
{
    public class RenderSystem : InternalSingleton<RenderSystem>
    {
        private List<BaseRenderer> m_renderableList;

        public RenderSystem()
        {
            m_renderableList = new List<BaseRenderer>();
        }

        public void RegisterRenderer(BaseRenderer renderer)
        {
            m_renderableList.Add(renderer);
            renderer.Initialize();
        }

        internal void RenderAllForCamera(Camera camera)
        {
            foreach(var renderType in m_renderableList)
            {
                renderType.Render(camera);
            }
        }

        internal void PostRenderUpdate()
        {
            foreach (var renderType in m_renderableList)
            {
                renderType.PostRenderUpdate();
            }
        }
    }
}
