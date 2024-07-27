using CPU_Doom;
using CPU_Doom.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    internal class SizedEnumTest
    {
        class BetterList<T> : SizedEnum<T>
        {
            private List<T> list = new List<T>();

            public BetterList(List<T> values) { list = values; }
            public T this[int key] => Get(key);
            public override T Get(int key) => list[key];
            public T this[long key] { set => list[(int)key] = value; }
            public override int Size => list.Count;
        }



        [Test]
        public void SizedEnum_ThouroughTest()
        {
            List<int> actual = new List<int>() { 1, 2, 3, 4, 5 }; 
            BetterList<int> betterList = new BetterList<int>(actual);
            List<int> expected = new List<int>();
            foreach (int value in betterList) 
            {
                expected.Add(value);
            }
            Assert.That(actual, Is.EqualTo(expected));
        }


    }
}
