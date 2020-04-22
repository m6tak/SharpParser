using System;

namespace SharpParser.Client
{
    class Program
    {
        // to test go to SharpParser.Client project properties and in Debug tab add arguments
        static void Main(string[] args)
        {
            var options = new Parser(args)
                .ParseWith<Options>()
                .OnError(ErrorHandler)
                .RetrieveOptions<Options>();

            Console.WriteLine(options.ToString());

            // do stuff with parsed arguments...
        }

        static void ErrorHandler(string error)
        {
            Console.WriteLine($"Error! {error}");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
