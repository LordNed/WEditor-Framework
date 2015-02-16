using OpenTK;
using System.Windows.Forms;
using System;

namespace WEditor
{
    public partial class EditorCore
    {
        /// <summary>
        /// Set the state of the specified key inside the input system. Use this to set
        /// whether a key is pressed or not in the host application.
        /// </summary>
        /// <param name="keyCode">Keycode of the key that was pressed.</param>
        /// <param name="bPressed">If the key was pressed down or released.</param>
        public void InputSetKeyState(Keys keyCode, bool bPressed)
        {
            Input.Internal_SetKeyState(keyCode, bPressed);
        }

        /// <summary>
        /// Set the state of the mouse buttons inside the input system. Use this to set
        /// the state of the mouse based on the host application.
        /// </summary>
        /// <param name="button">MouseButton of the pressed button. Supports
        /// LMB, RMB, MMB.</param>
        /// <param name="bPressed">Whether or not the button was pressed.</param>
        public void InputSetMouseBtnState(MouseButtons button, bool bPressed)
        {
            Input.Internal_SetMouseBtnState(button, bPressed);
        }

        /// <summary>
        /// Set the position of the mouse inside the input system. Use this to set the 
        /// mouse position based on the host application.
        /// </summary>
        /// <param name="mousePos">Position of mouse in pixels relative to the drawing viewport.</param>
        public void InputSetMousePos(Vector2 mousePos)
        {
            Input.Internal_SetMousePos(mousePos);
        }
    }
}
