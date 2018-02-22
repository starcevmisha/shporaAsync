using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SimpleClusterClient : ClusterClientBase
    {
        public SimpleClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
            
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            
            var resultTasks = new List<Task<string>>();
            for (var i = 0; i < ReplicaAddresses.Length; i++)
            {
                var uri = ReplicaAddresses[i];
                var webRequest = CreateRequest(uri + "?query=" + query);            
                Log.InfoFormat("Processing {0}", webRequest.RequestUri);
                resultTasks.Add(ProcessRequestAsync(webRequest));
            }

            Task<string> firstFinishedTask = await Task.WhenAny(resultTasks);


            return firstFinishedTask.Result;
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
    
    public static class TaskExtension{
        public static Task<T> WithCancellation<T>(this Task<T> task, CancellationTokenSource cancellationToken)
        {
            return task.IsCompleted // fast-path optimization
                ? task
                : task.ContinueWith(
                    completedTask => completedTask.GetAwaiter().GetResult(),
                    cancellationToken.Token,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
        }
    }
}