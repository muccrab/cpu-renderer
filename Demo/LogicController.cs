using Demo.Base_Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    internal class LogicController
    {
        public GameController GameController => _gameController;
        public Camera MainCamera { get; private set; }

        Dictionary<string, GameObject> _loadedScenes = new Dictionary<string, GameObject>();
        GameController _gameController;
        List<Camera> _cameras = new List<Camera>();
        Camera _baseCamera;

        public LogicController(GameController gameController)
        {
            _gameController = gameController;
            GameObject baseCameraObject = new GameObject();
            _baseCamera = new Camera();
            MainCamera = _baseCamera;
            baseCameraObject.AddComponent(_baseCamera, "Camera");
            baseCameraObject.Initialize(this);
        }
        public void Update()
        {
            foreach (var scene in _loadedScenes.Values) {
                scene.Update(); 
            }
        }
        public void LoadScene(string sceneName, GameObject scene)
        {
            if (scene.IsInitialized()) throw new Exception($"The scene {sceneName} can't be Initialized before loading");
            if (_loadedScenes.ContainsKey(sceneName)) throw new Exception($"The scene {sceneName} is already loaded");
            try { 
                scene.Initialize(this);
                scene.Start();
            }
            catch { throw; }
            _loadedScenes[sceneName] = scene;
        }

        public void UnloadScene(string sceneName) 
        {
            if (!_loadedScenes.ContainsKey(sceneName))
            {
                Console.WriteLine($"The scene {sceneName} is not currently Loaded");
                return;
            }
            _loadedScenes[sceneName].Destroy();
            _loadedScenes.Remove(sceneName);
        }

        public void AddCamera(Camera camera)
        {
            if (_cameras.Count == 0 || camera.Order < MainCamera.Order)
                MainCamera = camera;
            _cameras.Add(camera);
        }

        public void SetMainCameraIfLowOrder(Camera camera)
        {
            if (_cameras.Count <= 0) return;
            if (camera.Order < MainCamera.Order) MainCamera = camera;
        }

        public void RemoveCamera(Camera camera)
        {
            _cameras.Remove(camera);
            if (camera != MainCamera) return;
            if (_cameras.Count == 0) 
            { 
                MainCamera = _baseCamera;
                return;
            }
            int lowestPrio = int.MaxValue;
            foreach (var cam in _cameras)
            {
                if (cam.Order <= lowestPrio)
                {
                    lowestPrio = cam.Order;
                    MainCamera = cam;
                }
            }
        }
    }
}
