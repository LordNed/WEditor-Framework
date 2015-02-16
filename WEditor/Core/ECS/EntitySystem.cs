using System;
using System.Collections.Generic;
namespace WEditor
{
    internal class EntitySystem
    {
        private List<WEditorObject> m_editorObjects;

        private List<IUpdate> m_updateList;
        private List<ILateUpdate> m_lateUpdateList;

        public EntitySystem()
        {
            m_editorObjects = new List<WEditorObject>();
            m_updateList = new List<IUpdate>();
            m_lateUpdateList = new List<ILateUpdate>();
        }

        public void RegisterEditorObject(WEditorObject obj)
        {
            m_editorObjects.Add(obj);
        }

        public void RegisterComponent(BaseComponent component)
        {
            // Figure out which interfaces it implements and store references to them
            // in our lists so we can updat ethem later.
            IUpdate update = component as IUpdate;
            ILateUpdate lateUpdate = component as ILateUpdate;


            // Add any that aren't null.
            if (update != null) m_updateList.Add(update);
            if (lateUpdate != null) m_lateUpdateList.Add(lateUpdate);
        }

        public void ProcessFrame()
        {
            // Update all components that implement the IUpdate interface
            for (int i = 0; i < m_updateList.Count; i++)
                m_updateList[i].Update();

            // Update all components that implement the ILateUpdate interface
            for (int i = 0; i < m_lateUpdateList.Count; i++)
                m_lateUpdateList[i].LateUpdate();
        }
    }
}
