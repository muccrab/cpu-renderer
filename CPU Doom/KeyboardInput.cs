using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
