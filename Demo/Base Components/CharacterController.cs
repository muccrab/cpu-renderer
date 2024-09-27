using OpenTK.Mathematics;

namespace Demo.Base_Components
{
    internal class CharacterController : ObjectComponent
    {
        Vector3 initPos;
        float speed = 10;
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
            keyboard.MakeKey(ActionType.PRESS, SFML.Window.Keyboard.Key.R, Reset);
            initPos = ParentObject.Transform.Position;
        }

        private void MoveLeft() => ParentObject.Transform.Position += new Vector3(1, 0, 0) * speed * Time.DeltaTime;
        private void MoveRight() => ParentObject.Transform.Position += new Vector3(-1, 0, 0) * speed * Time.DeltaTime;

        private void MoveUp() => ParentObject.Transform.Position += new Vector3(0, -1, 0) * speed * Time.DeltaTime;

        private void MoveDown() => ParentObject.Transform.Position += new Vector3(0, 1, 0) * speed * Time.DeltaTime;

        private void MoveFront() => ParentObject.Transform.Position += new Vector3(0, 0, 1) * speed * Time.DeltaTime;

        private void MoveBack() => ParentObject.Transform.Position += new Vector3(0, 0, -1) * speed * Time.DeltaTime;

        private void Reset() => ParentObject.Transform.Position = initPos;
    }
}
