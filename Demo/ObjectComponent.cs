using CPU_Doom.Shaders;

namespace Demo
{
    internal abstract class ObjectComponent
    {
        public bool Active 
        { 
            get => _active; 
            set 
            {
                if (value == _active) return;
                _active = value;
                if (_active) OnActivate();
                else OnDeactivate();
            } 
        }
        public GameObject? ParentObject => _parentObject;
        public void Initialize(GameObject parentObject)
        {
            _parentObject = parentObject;
        }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void Destroy() { }

        public void SetShader(ShaderProgram shader) 
        {
            if (Active) OnSetShader(shader);
        }
        protected virtual void OnSetShader(ShaderProgram shader) { }

        protected virtual void OnDeactivate() { }
        protected virtual void OnActivate() { }

        GameObject? _parentObject;
        bool _active = true;
    }
}
