namespace WEditor
{
    public struct Rect
    {
        public float Width;
        public float Height;
        public float X;
        public float Y;

        public Rect(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override string ToString()
        {
            return string.Format("(x: {2} y: {3} width: {0} height: {1})", Width, Height, X, Y);
        }
    }
}
