using System.Threading.Tasks;
using Contracts;
using MassTransit;
using OpenTracing;
using Service2.Common;

namespace Service2
{
    public class EventFromService1Consumer : IConsumer<EventFromService1>
    {
        private readonly ITracer _tracer;

        public EventFromService1Consumer(ITracer tracer)
        {
            _tracer = tracer;
        }

        public async Task Consume(ConsumeContext<EventFromService1> context)
        {
            using (var scope = TracingExtension.StartServerSpan(_tracer, context.Message.TracingKeys, "event-from-service1-consumer"))
            {
                //some business logics

                await System.Console.Out.WriteLineAsync($"Message sent for {context.Message.Message}");
            }
        }
    }
}