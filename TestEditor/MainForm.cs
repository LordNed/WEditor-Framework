using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Windows.Forms;
using WEditor;

namespace TestEditor
{
    public partial class MainForm : Form
    {
        private EditorCore m_editorCore;

        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            // Create the Editor Core.
            m_editorCore = new EditorCore(glControl1.Width, glControl1.Height);

            // Wait until the MainForm has loaded before we register our event handlers.
            glControl1.KeyDown += HandleEventKeyDown;
            glControl1.KeyUp += HandleEventKeyUp;
            glControl1.MouseDown += HandleEventMouseDown;
            glControl1.MouseUp += HandleEventMouseUp;
            glControl1.Resize += HandleViewportResize;

            // Create a high-resolution polling timer since WinForm events for mice are so slow.
            Timer t1 = new Timer();
            t1.Interval = 16; //60-ish FPS
            t1.Tick += (o, args) =>
            {
                // Poll the mouse at a high resolution
                Point mousePos = glControl1.PointToClient(Cursor.Position);

                // Then, ensure we clamp it to screen-space of the Viewport before we pass it to
                // the editor core.
                mousePos.X = MathE.Clamp(mousePos.X, 0, glControl1.Width);
                mousePos.Y = MathE.Clamp(mousePos.Y, 0, glControl1.Height);

                m_editorCore.InputSetMousePos(new Vector2(mousePos.X, mousePos.Y));

                // Run a frame
                ProcessFrame();

                // Update the debug readout label.
                label1.Text = string.Format("Pos: {0}, {1}, Delta: {2}, {3}",
                    Input.MousePosition.X, Input.MousePosition.Y, Input.MouseDelta.X, Input.MouseDelta.Y);
            };
            t1.Enabled = true;


            // Test bed.
            WEditorObject testCube = WEditor.Utilities.Primitives.CreateCube(new Vector3(5, 5, 5));
            testCube.Name = "Test Cube";
        }

        private void HandleViewportResize(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            m_editorCore.ViewportResize(control.Width, control.Height);
        }

        private void HandleEventKeyDown(object sender, KeyEventArgs e)
        {
            m_editorCore.InputSetKeyState(e.KeyCode, true);
        }

        private void HandleEventKeyUp(object sender, KeyEventArgs e)
        {
            m_editorCore.InputSetKeyState(e.KeyCode, false);
        }

        private void HandleEventMouseDown(object sender, MouseEventArgs e)
        {
            m_editorCore.InputSetMouseBtnState(e.Button, true);
        }

        private void HandleEventMouseUp(object sender, MouseEventArgs e)
        {
            m_editorCore.InputSetMouseBtnState(e.Button, false);
        }

        private void ProcessFrame()
        {
            // Clear the backbuffer
            GL.ClearColor(glControl1.BackColor);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Update the editor core
            m_editorCore.ProcessFrame();

            // Swap the backbuffers so any drawing done by the editor core
            // show up.
            glControl1.SwapBuffers();
        }
    }
}
