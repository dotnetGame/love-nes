using System;

namespace LoveNes.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var system = new NesSystem();
            system.PowerUp();
        }
    }
}
