using System;

namespace WEditor
{
    public class InternalSingleton<T> where T : InternalSingleton<T>
    {
        private static T _cachedCopy;
        public InternalSingleton()
        {
            if (_cachedCopy != null)
            {
                Console.WriteLine("[WEditor.Core] Attempted to create duplicate singleton of type {0}!", typeof(T));
                return;
            }
            _cachedCopy = (T)this;
        }

        internal static T Instance
        {
            get { return _cachedCopy; }
        }
    }
}
