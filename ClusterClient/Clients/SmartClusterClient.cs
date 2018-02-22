using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {
        private Random rand = new Random();

        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var randomOrder = Enumerable
                .Range(0, ReplicaAddresses.Length)
                .OrderBy(c => rand.Next(100))
                .ToList();

            var timeoutOneTask = new TimeSpan(timeout.Ticks / ReplicaAddresses.Length);

            var resultTasks = new List<Task>();
            

            
            foreach (var i in randomOrder)
            {
                var uri = ReplicaAddresses[i];
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat("Processing {0}", webRequest.RequestUri);
                var resultTask = ProcessRequestAsync(webRequest);
                var timerTask = Task.Delay(timeoutOneTask);
                resultTask.Start();
                resultTasks.Add(resultTask);
                resultTasks.Add(timerTask);
                timerTask.Start();
                await Task.WhenAny(resultTasks);
                if (!resultTask.IsCompleted)
                    continue;
                return resultTask.Result;
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
}