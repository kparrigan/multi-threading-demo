using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiThreadingDemo.Domain.Enums;

namespace MultiThreadingDemo.Domain
{
    /// <summary>
    /// Dummy entity to use in simulated processing.
    /// </summary>
    public sealed class DataEntity
    {
        /// <summary>
        /// Id of entity.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Current Processing Status
        /// </summary>
        public ProcessingStatus ProcessingStatus { get; set; }
    }
}
