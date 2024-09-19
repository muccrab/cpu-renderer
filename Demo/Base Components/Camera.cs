using CPU_Doom.Shaders;
using OpenTK.Mathematics;


namespace Demo.Base_Components
{
    internal class Camera : ObjectComponent
    {
        public const float ASPECT = ((float)GameController.WIDTH) / GameController.HEIGHT; 
        public float FieldOfView { get; set; } = 60;
        public float DepthNear { get; set; } = 0.1f;
        public float DepthFar { get; set; } = 100f;
        public int Order 
        { 
            get => _order; 
            set 
            {
                _order = value;
                var logic = ParentObject?.LogicController;
                if (logic is null) return;
                logic.SetMainCameraIfLowOrder(this);
            } 
        }

        public override void Start()
        {
            var logic = ParentObject?.LogicController;
            if (logic is null) return;
            logic.AddCamera(this);
        }

        public override void Destroy()
        {
            var logic = ParentObject?.LogicController;
            if (logic is null) return;
            logic.RemoveCamera(this);
        }

        protected override void OnSetShader(ShaderProgram shader) 
        {
            if (ParentObject == null) return;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(FieldOfView * MathF.PI / 180, ASPECT, DepthNear, DepthFar);
            Matrix4 view = ParentObject.Transform.GetModelMatrix().Inverted();
            view.Transpose();
            shader.SetUniform("u_Projection", projection);
            shader.SetUniform("u_View", view);
        }

        private int _order = 0;
    }
}
