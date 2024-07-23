using CPU_Doom.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    struct MyData
    {
        public int a;
        public float b;
        public byte c;
        public double d;
    }


    internal class TypeConversionTests
    {
        [Test]
        public void ByteArrayToInt_SameBytes() 
        {
            byte[] b = { 15, 255, 64, 2 };
            int actual = b.AssignByteArrayToValue<int>();
            int expected = 37814031;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ByteArrayToInt_LessBytes()
        {
            byte[] b = { 64, 2 };
            int actual = b.AssignByteArrayToValue<int>();
            int expected = 576;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ByteArrayToInt_MoreBytes()
        {
            byte[] b = { 15, 255, 64, 2, 20, 105, 49, 74 };
            int actual = b.AssignByteArrayToValue<int>();
            int expected = 37814031;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void ByteArrayToVector_SameBytes()
        {
            byte[] b = { 15, 255, 64, 2 };
            OpenTK.Mathematics.Vector2i actual = b.AssignByteArrayToValue<OpenTK.Mathematics.Vector2i>();
            var expected = new OpenTK.Mathematics.Vector2i(37814031, 0);
            Assert.That(actual, Is.EqualTo(expected));
        }

    }
}
