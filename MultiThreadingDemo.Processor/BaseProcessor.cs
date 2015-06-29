using MultiThreadingDemo.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadingDemo.Processor
{
    public abstract class BaseProcessor
    {
        #region Member Variables
        /// <summary>
        /// Main processing task.
        /// </summary>
        private Task _mainTask;

        /// <summary>
        /// Interval to poll data layer for newly submitted entities.
        /// </summary>
        private Int32 _pollingIntervalMinutes;
        #endregion

        #region Properties
        /// <summary>
        /// Data access layer.
        /// </summary>
        protected IMultiThreadingDemoDal Dal { get; private set; }

        /// <summary>
        /// Current working set of entities to process.
        /// </summary>
        protected ConcurrentDictionary<Guid, Boolean> WorkingSet { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Cancellation token.
        /// </summary>
        protected CancellationTokenSource CancellationSource {get; set; }

        /// <summary>
        /// Instantiates a new instance of the <see cref="BaseProcessor" class./>
        /// </summary>
        /// <param name="dataAccessLayer">Data access layer.</param>
        /// <param name="pollingIntervalMinutes">Polling interval.</param>
        public BaseProcessor(IMultiThreadingDemoDal dataAccessLayer, Int32 pollingIntervalMinutes)
        {
            Dal = dataAccessLayer;
            _pollingIntervalMinutes = pollingIntervalMinutes;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Main processing entry point.
        /// </summary>
        public void Start()
        {
            Trace.WriteLine("Starting processing.");
            CancellationSource = new CancellationTokenSource();
            _mainTask = new Task(Poll, CancellationSource.Token, TaskCreationOptions.LongRunning);
            _mainTask.Start();
        }

        public void Stop()
        {
            Trace.WriteLine("Stopping processing.");
            CancellationSource.Cancel();
            _mainTask.Wait();
        }
        #endregion

        #region Constructor

        #endregion

        #region Private Methods
        private void Poll()
        {
            var token = CancellationSource.Token;
            TimeSpan interval = TimeSpan.FromMinutes(_pollingIntervalMinutes);
            while (!token.WaitHandle.WaitOne(interval))
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                RetrieveEntities();
            }
        }
        #endregion

        #region Abstract
        protected abstract void RetrieveEntities();
        #endregion
    }
}
