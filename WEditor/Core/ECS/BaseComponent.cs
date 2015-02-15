namespace WEditor
{ 
    public class BaseComponent
    {
        public WEditorObject EditorObject { get; private set; }

        internal BaseComponent(WEditorObject owner)
        {
            EditorObject = owner;
        }
    }
}
