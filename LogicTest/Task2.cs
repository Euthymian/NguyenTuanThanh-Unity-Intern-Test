using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicTest
{
    struct Cell
    {
        public int x, y, value;
        public Cell(int x, int y, int value)
        {
            this.x = x;
            this.y = y;
            this.value = value;
        }
    }

    internal class Task2
    {
        public static int Solution(int[][] a)
        {
            int res = 0;

            List<Cell> l = new List<Cell>();
            for(int y=0; y<a.Length; y++)
            {
                for(int x=0; x<a[y].Length; x++)
                {
                    l.Add(new Cell(x, y, a[y][x]));
                }
            }

            l.Sort(delegate (Cell c1, Cell c2) { return c1.value.CompareTo(c2.value); });

            for(int i = l.Count - 1; i >= 1; i--)
            {
                Cell r1 = l[i];
                for(int j=i-1; j>=0; j--)
                {
                    Cell r2 = l[j];
                    if(r1.x != r2.x && r1.y != r2.y)
                    {
                        res = int.Max(res, r1.value + r2.value);
                        break;
                    }
                }
            }

            return res;
        }
    }
}
