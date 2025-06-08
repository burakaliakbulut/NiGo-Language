using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int[] numbers = { 3, 5, 7, 8, 9, 10, 11, };
            for (int i = 0; i < numbers.Length; i++)
            {
                Console.WriteLine($"numbers[{i}]: {numbers[i]}");
            }
            Console.ReadKey();
        }
    }
}
