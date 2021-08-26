
using System;

namespace SystemMonitoring.Lambda
{
    public class UsageInfo
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string StatusCode { get; set; }
        public string Exception { get; set; }
        public string Function { get; set; }
        public DateTime DateTime { get; set; }
        public string State { get; set; }
        public string System { get; set; }
        public string Environment { get; set; }
        public string Method { get; set; }
    }
}
