using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom.Types
{
    public struct Pair<T1, T2>
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }
        public Pair(T1 value1, T2 value2) {  Value1 = value1; Value2 = value2; }
    }
}
