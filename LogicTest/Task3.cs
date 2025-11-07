using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTest
{
    internal class Task3
    {
        public static int Solution(int[] a)
        {
            int res = 0;
            Array.Sort(a);

            for(int i=0;i<a.Length; i++)
            {
                int o = i + 1;
                res += Math.Abs(o - a[i]);
            }

            return res;
        }
    }
}
