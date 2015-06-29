using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiThreadingDemo.Domain;
using MultiThreadingDemo.Domain.Enums;

namespace MultiThreadingDemo.Data
{
    public sealed class MultiThreadingDemoDal : IMultiThreadingDemoDal
    {
        #region Constants
        private const int MAX_ENTITIES_TO_ADD = 1000;

        private const int ENTITY_ADDITION_INTERVAL = 30;
        #endregion

        #region Properties
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

        /// <summary>
        /// Connection string to database.
        /// </summary>
        private String _connectionString;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolDemoDal" class./>
        /// </summary>
        /// <param name="connectionString">Connection string for the database.</param>
        public MultiThreadingDemoDal(string connectionString)
        {
            _connectionString = connectionString;
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolDemoDal" class./>
        /// </summary>
        /// <param name="connectionString">Connection string for the database.</param>
        /// <param name="maxEntityAdditions">Maximum number of times to add entities to the database.</param>
        public MultiThreadingDemoDal(string connectionString, Int32 maxEntityAdditions)
        {
            _connectionString = connectionString;
            _maxEntityAdditions = maxEntityAdditions;
            Init();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the member variables used to adding entities to the database.
        /// </summary>
        private void Init()
        {
            _rand = new Random((int)System.DateTime.Now.Ticks);
            _entityAdditionTimer = new Timer(AddEntities, null, TimeSpan.FromSeconds(ENTITY_ADDITION_INTERVAL),
                TimeSpan.FromSeconds(ENTITY_ADDITION_INTERVAL));
        }

        /// <summary>
        /// Adds a new batch of entities to the data layer.
        /// </summary>
        /// <param name="stateInfo">State info for timer callback.</param>
        private void AddEntities(Object stateInfo)
        {
            if (!_maxEntityAdditions.HasValue || (_maxEntityAdditions.HasValue && _maxEntityAdditions > _entityAdditions))
            {
                try
                {
                    // Add a random number of entities to the database.
                    var entityCount = _rand.Next(1, MAX_ENTITIES_TO_ADD + 1);
                    for (var i = 0; i < entityCount; i++)
                    {
                        var guid = Guid.NewGuid();

                        var parms = new DynamicParameters();
                        parms.Add("@id", guid);
                        parms.Add("@processingStatus", ProcessingStatus.Submitted.ToString());

                        using (var conn = new SqlConnection(_connectionString))
                        {
                            conn.Execute("dbo.InsertEntity", parms, commandType: CommandType.StoredProcedure);
                        }
                    }

                    _entityAdditions++;
                    Trace.WriteLine(String.Format("Added {0} new entities to database.", entityCount));
                }
                catch(SqlException ex)
                {
                    Trace.TraceError("Error adding entities to the database: " + ex.StackTrace);
                }
                catch(Exception ex)
                {
                    Trace.TraceError("Error adding entities to the database: " + ex.StackTrace);
                }
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
            var parms = new DynamicParameters();
            parms.Add("@id", entityId);
            parms.Add("@processingStatus", processingStatus.ToString());

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Execute("dbo.UpdateEntity", parms, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Retrieves all entities currently in a "submitted" status.
        /// </summary>
        /// <returns>Enumerable of entities.</returns>
        public IEnumerable<DataEntity> GetSubmittedEntities()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                return conn.Query<DataEntity>("dbo.GetSubmittedEntities",commandType: CommandType.StoredProcedure);
            }
        }
        #endregion
    }
}
