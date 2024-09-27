using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.Base_Components
{
    internal class DemoSwitcher : ObjectComponent
    {
        List<GameObject> _loadedScenes = new List<GameObject>();
        int _current = 0;

        public override void Start()
        {
            if (ParentObject == null) return;
            KeyboardMC? keyboard = ParentObject.GetComponent<KeyboardMC>();

            if (keyboard == null)
            {
                Console.WriteLine("The Keyboard component not found on the gameobject");
                return;
            }
            keyboard.MakeKey(ActionType.RELEASE, SFML.Window.Keyboard.Key.Right, NextScene);
            keyboard.MakeKey(ActionType.RELEASE, SFML.Window.Keyboard.Key.Left, PreviousScene);
            LoadSceneTriangle();
            LoadSceneMonke();
            LoadScenePyramid();
            _loadedScenes[0].SetParent(ParentObject);
        }

        private void LoadSceneTriangle()
        {
            var scene = new GameObject();
            var triangle = new GameObject();
            triangle.AddComponent(new Triangle());
            triangle.SetParent(scene);
            _loadedScenes.Add(scene);
        }

        private void LoadSceneMonke()
        {
            var scene = new GameObject();
            var monke = new GameObject();
            monke.AddComponent(new Monke());
            monke.SetParent(scene);
            _loadedScenes.Add(scene);
        }

        private void LoadScenePyramid()
        {
            var scene = new GameObject();
            var pyramid = new GameObject();
            pyramid.AddComponent(new Mesh());
            pyramid.AddComponent(new Texture());
            pyramid.SetParent(scene);
            _loadedScenes.Add(scene);
        }

        private void NextScene()
        {
            if (_loadedScenes.Count == 0 || ParentObject is null) return;
            _loadedScenes[_current].RemoveParent();
            if (++_current >= _loadedScenes.Count) _current = 0;
            _loadedScenes[_current].SetParent(ParentObject);
        }

        private void PreviousScene()
        {
            if (_loadedScenes.Count == 0 || ParentObject is null) return;
            _loadedScenes[_current].RemoveParent();
            if (--_current < 0) _current = _loadedScenes.Count - 1;
            _loadedScenes[_current].SetParent(ParentObject);
        }
    }
}
