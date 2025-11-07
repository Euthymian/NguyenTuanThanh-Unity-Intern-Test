using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTest
{
    internal class Task1
    {
        public static string Solution(string s)
        {
            s = s.ToLower();

            for(int i = 0; i < s.Length-1; i++)
            {
                if (s[i] > s[i + 1])
                    return s.Remove(i, 1);
            }

            return s.Remove(s.Length-1,1);
        }
    }
}
