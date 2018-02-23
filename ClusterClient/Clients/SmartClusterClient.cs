using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using log4net;

namespace ClusterClient.Clients
{
    public class SmartClusterClient : ClusterClientBase
    {

        public SmartClusterClient(string[] replicaAddresses) : base(replicaAddresses)
        {
        }

        public override async Task<string> ProcessRequestAsync(string query, TimeSpan timeout)
        {
            var resultTasks = new List<Task>();

            var timeoutOneTask = new TimeSpan(timeout.Ticks / ReplicaAddresses.Length);

            var timer = Task.Delay(timeoutOneTask);

            resultTasks.Add(timer);

            for (int i = 0; i < ReplicaAddresses.Length; i++)
            {
                
                var uri = ReplicaAddresses[i];
                var webRequest = CreateRequest(uri + "?query=" + query);
                Log.InfoFormat("Processing {0}", webRequest.RequestUri);
                resultTasks.Add(ProcessRequestAsync(webRequest));
                
                var completed = await Task.WhenAny(resultTasks);
                Console.WriteLine(i);
                if (completed is Task<string>)
                {
                    await Task.WhenAll(resultTasks);//Почему без этого отсальные не продолжат работать?
                    return ((Task<string>)completed).Result;
                }

                Log.InfoFormat("Timeout!!!", webRequest.RequestUri);
                resultTasks[0] = Task.Delay(timeoutOneTask);
            }



            throw new TimeoutException();
        }

        protected override ILog Log => LogManager.GetLogger(typeof(RandomClusterClient));
    }
}