using WEditor.Rendering;

namespace WEditor
{
    [RequireComponent(typeof(Mesh))]
    public class MeshRenderer : BaseComponent
    {
        public Mesh Mesh;
        public override void Initialize()
        {
            Mesh = EditorObject.GetComponent<Mesh>();

            StaticMeshRenderer.RegisterMesh(this);
        }
    }
}
