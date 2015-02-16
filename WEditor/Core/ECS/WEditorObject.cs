using System;
using System.Collections.Generic;

namespace WEditor
{
    public class WEditorObject
    {
        public string Name;
        public Transform Transform { get; private set; }

        private List<BaseComponent> m_componentList;

        public WEditorObject()
        {
            m_componentList = new List<BaseComponent>();
            Name = "WEditorObject";
            Transform = AddComponent<Transform>();

            EditorCore.Instance.RegisterEditorObject(this);
        }

        public T AddComponent<T>() where T : BaseComponent, new()
        {
            // Create a new instance of the component. Generics don't support parameters
            // so we're going to use a little bit of a workaround. This allows us to
            // assign the WEditorObject that the component belongs to via the constructor
            // and allowing normal get/private set to work, without doing some other weird
            // hacks.
            T newInst = new T();
            newInst.EditorObject = this;

            // Now that we've created it, we're going to add it to our internal list of components.
            m_componentList.Add(newInst);

            // However, before we're done, we're going to check the newly created component for any RequireComponent attributes. This
            // makes it so that we can ensure that required components for the component that is being added are also added. 
            // This gurantees that a GetComponent call won't return null for a class that specifies it requires said component.
            RequireComponent reqCompAtrib = (RequireComponent)Attribute.GetCustomAttribute(typeof(T), typeof(RequireComponent));
            if(reqCompAtrib != null)
            {
                for (int i = 0; i < reqCompAtrib.RequiredComponents.Length; i++)
                {
                    Type curType = reqCompAtrib.RequiredComponents[i];

                    // Search the Component list manually since GetComponent requires a generic and we can't go from Type to Generic.
                    bool bFound = false;
                    for (int k = 0; k < m_componentList.Count; k++)
                    {
                        if (curType == m_componentList[k].GetType())
                        {
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound)
                    {
                        AddComponentUnsafe(curType);
                    }
                }
            }

            EditorCore.Instance.RegisterComponent(newInst);
            newInst.Initialize();
            return newInst;
        }

        /// <summary>
        /// This allows us to add a component by type. However, it's not type checked (which is why it's marked unsafe). This is only used
        /// to add a required component via Type instead of via Generic as there doesn't seem to be a way to go from Type to T (which is
        /// addmittedly logical.) To work around this, this duplicates the functionality of the above, sans type checking and sans 
        /// attribute checking.
        /// </summary>
        /// <param name="type">Type of a BaseComponent to add.</param>
        private void AddComponentUnsafe(Type type)
        {
            if(!type.IsSubclassOf(typeof(BaseComponent)))
            {
                throw new ArgumentException("[WEditor.Core] Tried to AddComponent by type with a Type not derived from BaseComponent!", "type");
            }

            BaseComponent newInst = (BaseComponent)Activator.CreateInstance(type);
            m_componentList.Add(newInst);

            EditorCore.Instance.RegisterComponent(newInst);
            newInst.Initialize();
        }
        public T GetComponent<T>() where T : BaseComponent
        {
            Type typeCache = typeof(T);
            for(int i = 0; i < m_componentList.Count; i++)
            {
                if (m_componentList[i].GetType() == typeCache)
                    return (T)m_componentList[i];
            }

            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
