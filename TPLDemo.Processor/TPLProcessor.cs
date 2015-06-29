using MultiThreadingDemo.Data;
using MultiThreadingDemo.Domain;
using MultiThreadingDemo.Domain.Enums;
using MultiThreadingDemo.Processor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TPLDemo.Processor
{
    public sealed class TPLProcessor : BaseProcessor, IProcessor
    {
        #region Member Variables
        /// <summary>
        /// Max number of seconds to simulate processing for a given entity.
        /// </summary>
        private Int32 _maxProcessingSeconds = 600;

        /// <summary>
        /// Random used to determine processing time for an entity.
        /// </summary>
        private Random _processingTime;
        #endregion

        #region Constructor
        /// <summary>
        /// Instantiates a new instance of the <see cref="ThreadPoolProcessor" class./>
        /// </summary>
        /// <param name="dataAccessLayer">Data access layer.</param>
        /// <param name="pollingIntervalMinutes">Polling interval.</param>
        /// /// <param name="maxProcessingSeconds">Polling interval.</param>
        public TPLProcessor(IMultiThreadingDemoDal dataAccessLayer, Int32 pollingIntervalMinutes = 1, Int32 maxProcessingSeconds = 600)
            : base(dataAccessLayer, pollingIntervalMinutes)
        {
            _maxProcessingSeconds = maxProcessingSeconds;
            Init();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes the working set and polling variables.
        /// </summary>
        private void Init()
        {
            Int32 workerThreads, ioThreads;

            ThreadPool.GetMaxThreads(out workerThreads, out ioThreads);
            WorkingSet = new ConcurrentDictionary<Guid, bool>(ioThreads, ioThreads);
            _processingTime = new Random((Int32)System.DateTime.Now.Ticks);            
        }

        /// <summary>
        /// Retrieves any "Submitted" entities from the data layer and adds them to the working set.        
        /// </summary>
        sealed protected override void RetrieveEntities()
        {
            try
            {
                if (WorkingSet.Values.Count(x => !x) == 0)
                {
                    var entities = Dal.GetSubmittedEntities().ToList();
                    foreach (var entity in entities)
                    {
                        var id = entity.Id;
                        if (!WorkingSet.TryAdd(id, false))
                        {
                            Trace.TraceError(String.Format("Error adding entity with id {0} to working set.", id));
                        }
                        else
                        {
                            Task.Factory.StartNew(() => ProcessEntity(entity), CancellationSource.Token);
                        }
                    }

                    Trace.WriteLine(String.Format("Added {0} entities to the working set.", entities.Count));                    
                }
                else
                {
                    Trace.WriteLine(String.Format("No entities added due to working set still not processed."));   
                }
            }
            catch(Exception ex)
            {
                Trace.TraceError("Error updating working set: " + ex.StackTrace);
            }
        }

        /// <summary>
        /// Processing callback for the thread pool.
        /// </summary>
        /// <param name="threadContext">Context object for the thread pool callback.</param>
        private void ProcessEntity(DataEntity threadContext)
        {
            var entity = threadContext as DataEntity;

            if (entity != null)
            {
                var id = entity.Id;

                try
                {
                    Dal.UpdateEntity(id, ProcessingStatus.Processing);
                    
                    if (!WorkingSet.TryUpdate(id, true, false))
                    {
                        Trace.WriteLine(String.Format("Error updating working set for entity {0}.", id));
                        Dal.UpdateEntity(id, ProcessingStatus.Failed);
                    }

                    Trace.WriteLine(String.Format("Processing entity: {0}", id));
                    var processingSleepTime = _processingTime.Next(1, _maxProcessingSeconds);
                    Thread.Sleep(TimeSpan.FromSeconds(processingSleepTime));
                    Dal.UpdateEntity(id, ProcessingStatus.Complete);

                    bool removed;
                    if (!WorkingSet.TryRemove(id, out removed))
                    {
                        Trace.WriteLine(String.Format("Error removing entity {0} from working set.", id));
                    }

                    Trace.WriteLine(String.Format("Processing Complete for entity: {0}", id));
                    Trace.WriteLine(String.Format("Current working set size: {0}", WorkingSet.Count));
                }
                catch (SqlException ex)
                {
                    Dal.UpdateEntity(id, ProcessingStatus.Failed);
                    Trace.TraceError(String.Format("Error processing entity {0}: {1}", id, ex.StackTrace));
                }
                catch (Exception ex)
                {
                    Dal.UpdateEntity(id, ProcessingStatus.Failed);
                    Trace.TraceError(String.Format("Error processing entity {0}: {1}", id, ex.StackTrace));
                }
            }
        }
        #endregion
    }
}
