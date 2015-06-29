using MultiThreadingDemo.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPLDemo.Processor;

namespace TPLDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var conn = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            var dal = new MultiThreadingDemoDal(conn, 5);
            var processor = new TPLProcessor(dal);
            processor.Start();
            Console.ReadKey();
        }
    }
}
