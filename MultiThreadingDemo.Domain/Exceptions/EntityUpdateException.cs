using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiThreadingDemo.Domain.Exceptions
{
    public sealed class EntityUpdateException : Exception
    {
        #region Constructor
        public EntityUpdateException(string message)
            :base(message)
        {

        }
        #endregion
    }
}
