using GeniView.Cloud.Controllers.API;
using Hangfire;
using Hangfire.Storage;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace GeniView.Cloud.Common
{
	/// <summary>
	/// Hang Fire scheduler
	/// </summary>
	public class HFScheduler
	{
		private static Logger _logger = LogManager.GetCurrentClassLogger();

		MQTTMsgParser _MQTTMsgParser = new MQTTMsgParser();

		public HFScheduler()
		{
		}

		public void Setting()
		{
			string every1Sec = "*/1 * * * * *";

			RemoveAllJobs();
			ClearAndCreateJobs();

			// LogApiController is resolved by Hangfire's built-in job activator,
			// which creates a DI scope per job execution via IServiceScopeFactory.
			RecurringJob.AddOrUpdate<LogApiController>(
				"ProcessLog",
				x => x.ProcessLogJob(CancellationToken.None),
				every1Sec);

			_logger.Info("HFScheduler: ProcessLog recurring job registered (every 1 sec).");
		}

        public string ClearAndCreateJobs()
		{
			StringBuilder sb = new StringBuilder();
			Stopwatch watch = Stopwatch.StartNew();

			try
			{

				_logger.Info($"HFScheduler ClearAndCreateJobs start");

				//_db.uspDrop_HangfireTemporaryTables();


				_logger.Info($"HFScheduler ClearAndCreateJobs Finish");
			}
			catch (Exception ex)
			{
				var msg = ex.ToString();
				_logger.Error($"HFScheduler ClearAndCreateJobs Exception:{msg}");
			}
			return sb.ToString();
		}

		/// <summary>
		/// Remove hang fire exist jobs
		/// </summary>
		private void RemoveAllJobs()
		{
            //Remoce any exit recurringJob
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
            }
        }

		public void TestDelay()
		{
			Thread.Sleep(30000);
		}

		public void GetAmount()
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var queues = monitoringApi.Queues();
			var toDelete = new List<string>();
		}

		public int ClearProcessJobs(string name, string queueName, int page)
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var enqueueCount = monitoringApi.EnqueuedCount(queueName);

			int deleteJobsCount = 0;
			int count = 0;

			while (count <= enqueueCount)
			{
				//var jobs = monitoringApi.ProcessingJobs(count, page).Where(x => x.Value.Job.Method.Name == name);
				var jobs = monitoringApi.ProcessingJobs(count, page);

				var jobIds = jobs.Select(x => x.Key).ToList();
				deleteJobsCount += jobIds.Count;

				foreach (var jobId in jobIds)
				{
					BackgroundJob.Delete(jobId);
				}

				count += page;
			}

			return deleteJobsCount;
		}

		public int ClearEnQueueJobs(string name, string queueName, int page)
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var enqueueCount = monitoringApi.EnqueuedCount(queueName);

			int deleteJobsCount = 0;
			int count = 0;

			while (count <= enqueueCount)
			{
				//var jobs = monitoringApi.EnqueuedJobs("default", count, page).Where(x => x.Value.Job.Method.Name == name);
				var jobs = monitoringApi.EnqueuedJobs("default", count, page);

				var jobIds = jobs.Select(x => x.Key).ToList();
				deleteJobsCount += jobIds.Count;
				foreach (var jobId in jobIds)
				{
					BackgroundJob.Delete(jobId);
				}

				count += page;
			}

			return deleteJobsCount;
		}

		public int ClearDeleteJobs(string name, string queueName, int page)
		{
			var monitoringApi = JobStorage.Current.GetMonitoringApi();
			var enqueueCount = monitoringApi.DeletedListCount();

			int deleteJobsCount = 0;
			int count = 0;

			while (count <= enqueueCount)
			{
				var jobs = monitoringApi.DeletedJobs(count, page).Where(x => x.Value.Job.Method.Name == name);
				var jobIds = jobs.Select(x => x.Key).ToList();
				deleteJobsCount += jobIds.Count;
				foreach (var jobId in jobIds)
				{
					var result = BackgroundJob.Delete(jobId);
				}

				count += page;
			}

			return deleteJobsCount;
		}



	}
}