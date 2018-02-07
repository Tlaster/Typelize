using System;
using System.Collections.Generic;
using System.IO;
using Sprache;

namespace Typelize.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var sampleTypeClass = File.ReadAllText("SampleTypeClass.txt");
            var types = TypeParser.Parser.Parse(sampleTypeClass);
            foreach (var typeClass in types)
            {
//                Console.WriteLine(typeClass);
                Console.WriteLine(typeClass.ToCSharp("Typelize.Sample"));
            }
        }
    }
}
