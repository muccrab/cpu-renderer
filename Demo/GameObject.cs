namespace Demo
{
    // 
    internal class GameObject
    {
        const string TRANSFORM = "transform"; // name of transform component in GameObject 
        public LogicController? LogicController => _logicController;
        public GameController? GameController => _logicController?.GameController;  
        public GameObject? Parent => _parent;
        public Transform3D Transform { get; private set; }
        public GameObject() {
            _componentTypes[typeof(Transform3D)] = TRANSFORM;
            Transform = new Transform3D();
            _components[TRANSFORM] = Transform;
        }


        public bool IsInitialized()
        {
            if (_initialized) return true;
            foreach (var child in _children)
            {
                if (child.IsInitialized()) return true;
            }
            return false;
        }
        public void Initialize(LogicController logicController)
        {
            foreach (var component in _components.Values) component.Initialize(this);
            foreach (var child in _children) child.Initialize(logicController);
            _initialized = true;
            _logicController = logicController;
        }

        public void Start() 
        {
            foreach (var component in _components.Values) component.Start();
            foreach (var child in _children) child.Start();
        }
        public void Update() 
        {
            foreach (var component in _components.Values) component.Update();
            foreach (var child in _children) child.Update();
        }
        public void Destroy() 
        {
            foreach (var component in _components.Values) component.Destroy();
            foreach (var child in _children) child.Destroy();
            _initialized = false;
        }

        public void SetParent(GameObject parent)
        {
            _parent?._children.Remove(this);
            parent._children.Add(this);
            _parent = parent;

        }
        public void AddComponent(ObjectComponent component, string? compID = null)
        {
            if (compID == null) compID = component.ToString() ?? "UnspecifiedComponent";
            if (_components.ContainsKey(compID)) throw new Exception($"Component with ID: {compID} already exists");
            if (!_componentTypes.ContainsKey(component.GetType())) _componentTypes.Add(component.GetType(), compID);
            _components[compID] = component;
        }
        public ObjectComponent GetComponent(string compID) => _components[compID];
        public ObjectComponent GetComponent(Type type) {

            if (_componentTypes.ContainsKey(type)) return _components[_componentTypes[type]];
            Type? oType = _componentTypes.Keys.Where(otype => otype.IsAssignableTo(otype)).FirstOrDefault();
            if (oType is not null) return _components[_componentTypes[oType]];
            throw new Exception($"Component does not have type {oType}");
                
        }
        public TType GetComponent<TType>() where TType : ObjectComponent
            => (TType)_components[_componentTypes[typeof(TType)]];


        private LogicController? _logicController;
        private GameObject? _parent;
        private Dictionary<string, ObjectComponent> _components = new Dictionary<string, ObjectComponent>();
        private Dictionary<Type, string> _componentTypes = new Dictionary<Type, string>();
        private List<GameObject> _children = new List<GameObject>();
        private bool _initialized = false;
    }
}
