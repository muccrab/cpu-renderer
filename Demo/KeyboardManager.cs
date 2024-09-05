using CPU_Doom;
using Demo.Base_Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    internal class KeyboardManager
    {
        private List<KeyboardMC> _keyboardComponent = new List<KeyboardMC>();
        public KeyboardManager(Window3D window)
        {
            window.KeyPress += OnKeyPressed;
            window.KeyReleased += OnKeyReleased;
        }

        public void AddKeyboardComponent(KeyboardMC keyboardComponent)
        {
            _keyboardComponent.Add(keyboardComponent);
        }
        public void RemoveKeyboardComponent(KeyboardMC keyboardComponent)
        {
            _keyboardComponent.Remove(keyboardComponent);
        }
        private void OnKeyPressed(KeyboardInput input)
        {
            foreach (KeyboardMC keyboard in _keyboardComponent) keyboard.OnKeyPressed(input);
        }
        private void OnKeyReleased(KeyboardInput input)
        {
            foreach (KeyboardMC keyboard in _keyboardComponent) keyboard.OnKeyReleased(input);
        }
    }
}
