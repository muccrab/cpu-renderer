using CPU_Doom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Base_Components
{
    enum ActionType
    {
        PRESS, HOLD, RELEASE
    }


    internal class KeyboardMC : ObjectComponent
    {
        public override void Start()
        {
            ParentObject?.GameController?.KeyboardManager.AddKeyboardComponent(this);
        }
        public override void Update() 
        {
            if (_holdedKeys.Count == 0) return;
            foreach (var key in _holdedKeys)
            {
                if (_onHold.ContainsKey(key)) _onHold[key]();
            }
        }
        public override void Destroy()
        {
            ParentObject?.GameController?.KeyboardManager.RemoveKeyboardComponent(this);
        }

        public void MakeKey(ActionType actionType, SFML.Window.Keyboard.Key key, Action action)
        {
            switch (actionType)
            {
                case ActionType.PRESS:
                    if (!_onPress.ContainsKey(key)) _onHold.Add(key, action);
                    else _onPress[key] += action;
                    break;
                case ActionType.RELEASE:
                    if (!_onRelease.ContainsKey(key)) _onHold.Add(key, action);
                    else _onRelease[key] += action;
                    break;
                case ActionType.HOLD:
                    if (!_onHold.ContainsKey(key)) _onHold.Add(key, action);
                    else _onHold[key] += action;
                    break;
            }
        }

        public void OnKeyPressed(KeyboardInput input)
        {
            if (!Active) return;
            var code = input.e.Code;
            _holdedKeys.Add(code);
            if (_onPress.ContainsKey(code)) _onPress[code]();
        }
        public void OnKeyReleased(KeyboardInput input)
        {
            var code = input.e.Code;
            _holdedKeys.Remove(code);
            if (!Active) return;
            if (_onRelease.ContainsKey(code)) _onRelease[code]();
        }

        Dictionary<SFML.Window.Keyboard.Key, Action> _onPress = new Dictionary<SFML.Window.Keyboard.Key, Action>();
        Dictionary<SFML.Window.Keyboard.Key, Action> _onRelease = new Dictionary<SFML.Window.Keyboard.Key, Action>();
        Dictionary<SFML.Window.Keyboard.Key, Action> _onHold = new Dictionary<SFML.Window.Keyboard.Key, Action>();

        HashSet<SFML.Window.Keyboard.Key> _holdedKeys = new HashSet<SFML.Window.Keyboard.Key>();

    }





}
