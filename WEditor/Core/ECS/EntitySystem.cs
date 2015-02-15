using System;
using System.Collections.Generic;
namespace WEditor
{
    internal class EntitySystem
    {
        private List<WEditorObject> m_editorObjects;

        public EntitySystem()
        {
            m_editorObjects = new List<WEditorObject>();
        }

        public void RegisterEditorObject(WEditorObject obj)
        {
            m_editorObjects.Add(obj);
        }

        public void ProcessFrame()
        {
            for(int i = 0; i < m_editorObjects.Count; i++)
            {
                WEditorObject obj = m_editorObjects[i];
            }
        }
    }
}
