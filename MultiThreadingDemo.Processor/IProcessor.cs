using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadingDemo.Processor
{
    public interface IProcessor
    {

        void Start();

        void Stop();
    }
}
