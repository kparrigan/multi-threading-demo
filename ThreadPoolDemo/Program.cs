using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiThreadingDemo.Data;
using ThreadPoolDemo.Processor;

namespace ThreadPoolDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //var dal = new InMemoryThreadPoolDemoDal(10);
            var conn = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            var dal = new MultiThreadingDemoDal(conn, 5);
            var processor = new ThreadPoolProcessor(dal);
            processor.Start();
            Console.ReadKey();
        }
    }
}
