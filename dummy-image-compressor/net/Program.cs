using System;
using McMaster.Extensions.CommandLineUtils;

namespace DummyImageCompressor
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication<Program>();

            var imageCompressor = new ImageCompressor();

            if (args.Length == 0)
            {
                Console.WriteLine("This utility compresses JPEG image files supplied as parameters.");
                Console.WriteLine("New compressed files will be placed in the original directory having '_' sign as a prefix.");
                Console.WriteLine("Default image dimensions are 1920x1080 . For custom dimensions size, use the following parameters (one or both) when calling the app: width=_here_goes_the_value_ height=_here_goes_the_value_ ");
            }
            else
            {
                imageCompressor.Compress(args);
            }


            Console.WriteLine();
            Console.WriteLine("********************************************************");
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }
    }
}
