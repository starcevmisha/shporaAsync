using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Patterns.LogParsers
{
    public class PLINQLogParser : ILogParser
    {
        public KeyValuePair<string, int>[] GetTop10Users(string logPath)
        {
            
            return File.ReadLines(logPath).AsParallel()
                .Select(IpInfo.Parse)
                .GroupBy(
                    g => g.Ip,
                    g => g.CallDuration,
                    
                    (key, durations) => new KeyValuePair<string, int>(key, Thread.CurrentThread.ManagedThreadId))
                .ToArray();
        }
    }
}