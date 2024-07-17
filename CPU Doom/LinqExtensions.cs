using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU_Doom
{
    public static class LinqExtensions
    {
        public static bool HasExactlyNElements<T>(this IEnumerable<T> en, int n)
        {
            using (var enumerator = en.GetEnumerator())
            {
                int count = 0;
                for (var i = 0; i < n; i++)
                {
                    if (enumerator.MoveNext()) count++;
                    else return false;
                }
                if (enumerator.MoveNext()) return false;
                return true;
            }
        }
        public static bool HasExactlyOneElement<T>(this IEnumerable<T> en) => HasExactlyNElements(en, 1);
        public static bool HasNoElements<T>(this IEnumerable<T> en) => HasExactlyNElements(en, 0);
        public static bool HasElements<T>(this IEnumerable<T> en) => !HasExactlyNElements(en, 0);


        public static List<(T1?, T2?)> OuterJoin<T1, T2>(this IEnumerable<T1> e1, IEnumerable<T2> e2, Func<T1, T2, bool> cond)
        {
            HashSet<T2> unfoundedE2s = new HashSet<T2>(e2);
            List<(T1?, T2?)> ret = new List<(T1?, T2?)>();

            foreach (var x in e1) 
            {
                foreach (var y in unfoundedE2s)
                {
                    if (cond(x,y))
                    {
                        ret.Add((x, y));
                        unfoundedE2s.Remove(y);
                        goto doublecontinue;
                    }
                }
                ret.Add((x, default));
            doublecontinue:;
            }
            foreach (var y in unfoundedE2s)
            {
                ret.Add((default, y));
            }
            return ret;
        }


    }
}
