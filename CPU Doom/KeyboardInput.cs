using SFML.Window;

namespace CPU_Doom
{
    public class KeyboardInput
    {
        public object? sender;
        public KeyEventArgs e;

        public KeyboardInput(object? sender, KeyEventArgs e)
        {
            this.sender = sender;
            this.e = e;
        }
    }
}
