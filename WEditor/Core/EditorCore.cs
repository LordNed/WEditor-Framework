using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using WEditor.Rendering;

namespace WEditor
{
    /// <summary>
    /// The EditorCore handles most of the editor logic. This allows you to use it in multiple contexts
    /// as long as you feed the appropriate events back into this. This decouples any UI from the actual
    /// editor logic which promotes a better, more portable code.
    /// </summary>
    public partial class EditorCore : InternalSingleton<EditorCore>
    {
        /// <summary> Used to calculate the delta time of each processed frame. </summary>
        private Stopwatch m_dtStopWatch;
        /// <summary> List of cameras currently in the scene. </summary>
        private List<Camera> m_cameraList;
        /// <summary> RenderSystem handles the rendering of objects, materials, shaders, etc. </summary>
        private RenderSystem m_renderSystem;
        /// <summary> EntitySystem handles <see cref="WEditorObject"/> and the rest of the ECS system. </summary>
        private EntitySystem m_entitySystem;

        public EditorCore(int viewportWidth, int viewportHeight)
        {
            // Delta Time stopwatch
            m_dtStopWatch = new Stopwatch();
            m_cameraList = new List<Camera>();
            m_renderSystem = new RenderSystem();
            m_entitySystem = new EntitySystem();

            // Set up the default Viewport Width/Height
            Display.Internal_EventResize(viewportWidth, viewportHeight);

            // Create a default camera
            WEditorObject defaultCamera = new WEditorObject();
            defaultCamera.Name = "EditorCamera";
            defaultCamera.Transform.Position = new OpenTK.Vector3(0, 0, -50);
            defaultCamera.AddComponent<Camera>();
            defaultCamera.AddComponent<FPSCameraMovement>();

            // Create our default mesh renderer
            m_renderSystem.RegisterRenderer(new StaticMeshRenderer());

            Console.WriteLine("[WEditor.Core] Initialized.");
        }

        /// <summary>
        /// This is the main editor application loop. This should be called each frame, and will handle
        /// input, rendering, etc.
        /// </summary>
        public void ProcessFrame()
        {
            // Calculate a new DeltaTime for this frame (time it took for the last frame to render)
            Time.Internal_UpdateTime(m_dtStopWatch.ElapsedMilliseconds / 1000f);
            m_dtStopWatch.Restart();

            // Process all WEditorObject and Components before we render.
            m_entitySystem.ProcessFrame();

            // Render things.
            GL.Enable(EnableCap.ScissorTest);
            foreach(var camera in m_cameraList)
            {
                GL.Viewport((int)camera.PixelRect.X, (int)camera.PixelRect.Y, (int)camera.PixelRect.Width, (int)camera.PixelRect.Height);
                GL.Scissor((int)camera.PixelRect.X, (int)camera.PixelRect.Y, (int)camera.PixelRect.Width, (int)camera.PixelRect.Height);

                // Clear the backbuffer
                GL.ClearColor(camera.ClearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                // Now render things...
                m_renderSystem.RenderAllForCamera(camera);
            }
            GL.Disable(EnableCap.ScissorTest);


            // Calculate the input for this frame (calculates button press/release, mouse press/release, input delta, etc.)
            Input.Internal_UpdateInputState();
        }

        public void ViewportResize(int newWidth, int newHeight)
        {
            //Console.WriteLine("[WEditor.Core] Viewport resized. New dimensions (width: {0} height: {1})", newWidth, newHeight);
            Display.Internal_EventResize(newWidth, newHeight);
        }

        internal void RegisterCamera(Camera cam)
        {
            m_cameraList.Add(cam);
        }

        internal void RegisterEditorObject(WEditorObject obj)
        {
            m_entitySystem.RegisterEditorObject(obj);
        }

        internal void RegisterComponent(BaseComponent component)
        {
            m_entitySystem.RegisterComponent(component);
        }
    }
}
