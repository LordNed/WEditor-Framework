namespace WEditor
{ 
    public abstract class BaseComponent
    {
        public WEditorObject EditorObject { get; internal set; }

        /// <summary>
        /// Use this function in the place of a constructor. Initialize will be the first thing called
        /// after a component is created.
        /// </summary>
        public virtual void Initialize() { }

        /// <summary>
        /// This function is provided as an ease of access to the Transform class since all objects will have a Transform.
        /// </summary>
        /// <returns></returns>
        public Transform GetTransform()
        {
            return EditorObject.Transform;
        }
    }
}
