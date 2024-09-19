namespace Demo.Base_Components
{
    internal class CharacterController : ObjectComponent
    {   
        public override void Start()
        {
            if (ParentObject == null) return;
            KeyboardMC? keyboard = ParentObject.GetComponent<KeyboardMC>();

            if (keyboard == null)
            {
                Console.WriteLine("The Keyboard component not found on the gameobject");
                return;
            }
            keyboard.MakeKey(ActionType.HOLD, SFML.Window.Keyboard.Key.A, MoveLeft);
            keyboard.MakeKey(ActionType.HOLD, SFML.Window.Keyboard.Key.D, MoveRight);
            keyboard.MakeKey(ActionType.HOLD, SFML.Window.Keyboard.Key.S, MoveDown);
            keyboard.MakeKey(ActionType.HOLD, SFML.Window.Keyboard.Key.W, MoveUp);
            keyboard.MakeKey(ActionType.HOLD, SFML.Window.Keyboard.Key.Up, MoveFront);
            keyboard.MakeKey(ActionType.HOLD, SFML.Window.Keyboard.Key.Down, MoveBack);
        }

        private void MoveLeft() => ParentObject.Transform.Position += new OpenTK.Mathematics.Vector3(1, 0, 0);
        private void MoveRight() => ParentObject.Transform.Position += new OpenTK.Mathematics.Vector3(-1, 0, 0);

        private void MoveUp() => ParentObject.Transform.Position += new OpenTK.Mathematics.Vector3(0, -1, 0);

        private void MoveDown() => ParentObject.Transform.Position += new OpenTK.Mathematics.Vector3(0, 1, 0);

        private void MoveFront() => ParentObject.Transform.Position += new OpenTK.Mathematics.Vector3(0, 0, 1);

        private void MoveBack() => ParentObject.Transform.Position += new OpenTK.Mathematics.Vector3(0, 0, -1);
    }
}
