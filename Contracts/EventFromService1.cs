using System.Collections.Generic;

namespace Contracts
{
    public class EventFromService1
    {
        public string Message { get; set; }
        public Dictionary<string, string> TracingKeys { get; set; }
    }
}