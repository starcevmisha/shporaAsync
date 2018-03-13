using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ClusterClient.Clients
{
    public class RoundRobinClusterClient : ClusterClientBase
    {
        private Random rand = new Random();

        public RoundRobinClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var randomOrder = Enumerable
                .Range(0, ReplicaAddresses.Length)
                .OrderBy(c => rand.Next(100))
                .ToList();

            var timeoutOneTask = new TimeSpan(timeout.Ticks / ReplicaAddresses.Length);
            
            foreach (var i in randomOrder)
            {
                var uri = ReplicaAddresses[i];
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat("Processing {0}", webRequest.RequestUri);
                var resultTask = ProcessRequestAsync(webRequest);
                await Task.WhenAny(resultTask, Task.Delay(timeoutOneTask));
                if (!resultTask.IsCompleted)
                    continue;
                return resultTask.Result;
            }

            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
}