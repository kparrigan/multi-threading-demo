using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MultiThreadingDemo.Domain;
using MultiThreadingDemo.Domain.Enums;
using System.Collections.Concurrent;
using MultiThreadingDemo.Domain.Exceptions;
using System.Diagnostics;

namespace MultiThreadingDemo.Data
{
    /// <summary>
    /// Fake data layer to simulate database interaction.
    /// </summary>
    public sealed class InMemoryMultiThreadingDemoDal : IMultiThreadingDemoDal
    {
        #region Constants
        private const int MAX_ENTITIES_TO_ADD = 100;
        #endregion

        #region Properties
        /// <summary>
        /// Entities
        /// </summary>
        private ConcurrentDictionary<Guid, DataEntity> _entities;
        /// <summary>
        /// Timer used to periodically add new entities to the data layer.
        /// </summary>
        private Timer _entityAdditionTimer;

        /// <summary>
        /// Random object used to add a variable amount of entities to the DAL.
        /// </summary>
        private Random _rand;

        /// <summary>
        /// Max entities to add to data layer.
        /// </summary>
        private Int32? _maxEntityAdditions;

        /// <summary>
        /// Number of times entities have been added.
        /// </summary>
        private Int32 _entityAdditions;
        #endregion

        #region Constructor
        public InMemoryMultiThreadingDemoDal()
        {
            Init();
        }

        public InMemoryMultiThreadingDemoDal(Int32 maxEntityAdditions)
        {
            _maxEntityAdditions = maxEntityAdditions;
            Init();
        }
        #endregion

        #region Private Methods
        private void Init()
        {
            _entities = new ConcurrentDictionary<Guid, DataEntity>();
            _rand = new Random((int)System.DateTime.Now.Ticks);
            _entityAdditionTimer = new Timer(AddEntities, null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30));
        }

        /// <summary>
        /// Adds a new batch of entities to the data layer.
        /// </summary>
        /// <param name="stateInfo"></param>
        private void AddEntities(Object stateInfo)
        {
            if (!_maxEntityAdditions.HasValue || (_maxEntityAdditions.HasValue && _maxEntityAdditions > _entityAdditions))
            {
                var entityCount = _rand.Next(1,MAX_ENTITIES_TO_ADD + 1);
                for (var i = 0; i < entityCount; i++)
                {
                    var guid = Guid.NewGuid();
                    _entities.TryAdd(guid, new DataEntity { Id = guid, ProcessingStatus = ProcessingStatus.Submitted });
                }

                _entityAdditions++;
                Trace.WriteLine(String.Format("Added {0} new entities to data layer.", entityCount));
            }
            else
            {
                Trace.WriteLine("Max entity additions reached.");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update an entity to a new status.
        /// </summary>
        /// <param name="entityId">Id of entity to update.</param>
        /// <param name="processingStatus">New status of entity.</param>
        public void UpdateEntity(Guid entityId, ProcessingStatus processingStatus)
        {
            DataEntity entity;
            if (!_entities.TryGetValue(entityId, out entity))
            {
                throw new ArgumentException("Entity does not exist.", "entityId");
            }
            
            if (!_entities.TryUpdate(entityId, new DataEntity {Id = entityId, ProcessingStatus = processingStatus}, entity))
            {
                throw new EntityUpdateException("Error updating entity.");
            }
        }

        /// <summary>
        /// Retrieves all entities currently in a "submitted" status.
        /// </summary>
        /// <returns>Enumerable of entities.</returns>
        public IEnumerable<DataEntity> GetSubmittedEntities()
        {
            return _entities.Values.Where(e => e.ProcessingStatus == ProcessingStatus.Submitted);
        }
        #endregion
    }
}
