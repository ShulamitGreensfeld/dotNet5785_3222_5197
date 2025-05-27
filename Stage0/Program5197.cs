using System;

namespace Stage0
{
    partial class Program
    {
        static void Main(string[] args)
        {
            welcome5197();
        }

        static partial void welcome5197()
        {
            Console.WriteLine("Enter your name:");
            string name = Console.ReadLine();
            Console.WriteLine("{0}, welcome to my first console application", name);
        }
    }
}