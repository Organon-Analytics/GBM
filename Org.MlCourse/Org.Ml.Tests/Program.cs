using Org.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Org.Ml.Tests
{
    class Program
    {
        public static void Main(string[] args)
        {
            var test = new GbmModelBuildServiceTest();
            test.Execute();

            Console.WriteLine("Computation finished");
            Console.ReadLine();
        }
    }
}
