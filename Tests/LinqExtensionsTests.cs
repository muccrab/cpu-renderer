using CPU_Doom;
namespace Tests
{
    public class LinqExtensionsTests
    {

        [Test]
        public void LinqExtensions_HasExactlyNElements_ZeroElements_Same()
        {
            List<int> list = new List<int>();
            Assert.IsTrue(list.HasNoElements());
        }

        [Test]
        public void LinqExtensions_HasExactlyNElements_ZeroElements_More()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4 };
            Assert.IsTrue(list.HasElements());
        }
        [Test]
        public void LinqExtensions_HasExactlyNElements_OneElements_Less()
        {
            List<int> list = new List<int>();
            Assert.IsFalse(list.HasExactlyOneElement());
        }
        [Test]
        public void LinqExtensions_HasExactlyNElements_OneElements_Same()
        {
            List<int> list = new List<int>() { 5 };
            Assert.IsTrue(list.HasExactlyOneElement());
        }
        [Test]
        public void LinqExtensions_HasExactlyNElements_OneElements_More()
        {
            List<int> list = new List<int>() { 1, 2, 3 };
            Assert.IsFalse(list.HasExactlyOneElement());
        }
        [Test]
        public void LinqExtensions_HasExactlyNElements_FiveElements_Less()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4 };
            Assert.IsFalse(list.HasExactlyNElements(5));
        }

        [Test]
        public void LinqExtensions_HasExactlyNElements_FiveElements_Same()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5 };
            Assert.IsTrue(list.HasExactlyNElements(5));
        }
        [Test]
        public void LinqExtensions_HasExactlyNElements_FiveElements_More()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.IsFalse(list.HasExactlyNElements(5));
        }

        [Test]
        public void LinqExtensions_OuterJoin_JoinTwoLists()
        {
            List<int> list1 = new List<int>() { 1, 2, 3, 4, 5 };
            List<int> list2 = new List<int>() { 1, 7, 3, 5, 9 };
            List<(int?, int?)> expected = new List<(int?, int?)>() 
            { (1, 1), (2, 0), (3,3), (4,0), (5,5), (0, 7), (0, 9) };
            var actual = list1.OuterJoin(list2, (x, y) => { return x == y; });
            Assert.That(actual, Is.EqualTo(expected));
        }

    }
}