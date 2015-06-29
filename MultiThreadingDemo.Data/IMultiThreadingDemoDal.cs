using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiThreadingDemo.Domain;
using MultiThreadingDemo.Domain.Enums;

namespace MultiThreadingDemo.Data
{
    public interface IMultiThreadingDemoDal
    {
        /// <summary>
        /// Update an entity to a new status.
        /// </summary>
        /// <param name="entityId">Id of entity to update.</param>
        /// <param name="processingStatus">New status of entity.</param>
        void UpdateEntity(Guid entityId, ProcessingStatus processingStatus);

        /// <summary>
        /// Retrieves all entities currently in a "submitted" status.
        /// </summary>
        /// <returns>Enumerable of entities.</returns>
        IEnumerable<DataEntity> GetSubmittedEntities();
    }
}
