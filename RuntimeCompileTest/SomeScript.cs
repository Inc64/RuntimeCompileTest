using System;

namespace RuntimeCompileTest
{
    public class SomeScript
    {
        public void Execute()
        {
            decimal A = 12;
            decimal B = 17.28293939m;

            double A1 = 7.8383;
            double B1 = 0.00001;

            Console.WriteLine();

            Console.WriteLine("hello from script!");
            Console.WriteLine($"decimal A = {A}");
            Console.WriteLine($"decimal B = {B}");
            Console.WriteLine($"A + B = {A + B}");
            Console.WriteLine($"A * B = {A * B}");
            Console.WriteLine($"A / B = {A / B}");

            Console.WriteLine($"double A1 = {A1}");
            Console.WriteLine($"double B1 = {B1}");
            Console.WriteLine($"A1 + B1 = {A1 + B1}");
            Console.WriteLine($"A1 * B1 = {A1 * B1}");
            
            Console.WriteLine();
        }
    }
}
