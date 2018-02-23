using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class CancellingClusterClient : ClusterClientBase
    {
        private Random rand = new Random();

        public CancellingClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var randomOrder = Enumerable
                .Range(0, ReplicaAddresses.Length)
                .OrderBy(c => rand.Next(100))
                .ToList();

            var timeoutOneTask = new TimeSpan(timeout.Ticks / ReplicaAddresses.Length);

            var token = new CancellationTokenSource();

            var resultTasks = new List<Task>();
            var timer = Task.Delay(timeout);
            for (var i = 0; i < ReplicaAddresses.Length; i++)
            {
                var uri = ReplicaAddresses[i];
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat("Processing {0}", webRequest.RequestUri);
                resultTasks.Add(ProcessRequestAsync(webRequest, token));
            }

            var firstFinishedTask = await Task.WhenAny(resultTasks);
            token.Cancel();
            if (firstFinishedTask == timer)
                throw new TimeoutException();
            return ((Task<string>) firstFinishedTask).Result;
        }


        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
}