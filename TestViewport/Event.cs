using OpenTK;

namespace TestViewport
{
    public enum EventType
    {
        None, MouseDown, MouseDrag, MouseUp,
        Layout, Used
    }

    public class Event
    {
        public static Event current;
        public EventType Type;
        public int button;
        public Vector3 mousePosition;
        public Vector2 delta;


        public Event()
        {
            Type = EventType.None;
            mousePosition = Vector3.Zero;
            delta = Vector2.Zero;
        }

        public void Use()
        {
            Type = EventType.Used;
        }
    }
}