using System;
using System.IO;
using System.Threading.Tasks;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders.Thrift;
using MassTransit;
using MetroBus;
using MetroBus.Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using Service2.Common;

namespace Service2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(basePath: Directory.GetCurrentDirectory());
                })
                .ConfigureServices((hostContext, services) =>
                {
                    //Init tracer
                    services.AddSingleton<ITracer>(t => InitTracer());

                    string rabbitMqUri = "rabbitmq://rabbitmq:5672";
                    string rabbitMqUserName = "admin";
                    string rabbitMqPassword = "123456";

                    services.AddMetroBus(x =>
                    {
                        x.AddConsumer<EventFromService1Consumer>();
                    });

                    services.AddSingleton<IBusControl>(provider => MetroBusInitializer.Instance
                        .UseRabbitMq(rabbitMqUri, rabbitMqUserName, rabbitMqPassword)
                        .RegisterConsumer<EventFromService1Consumer>("event.from.service1.queue", provider)
                        .Build());

                    services.AddHostedService<BusService>();
                });

            await host.RunConsoleAsync();
        }
        
        private static ITracer InitTracer()
        {
            var serviceName = "Service2";
            var loggerFactory = new LoggerFactory();;
                
            var reporter = new RemoteReporter.Builder()
                .WithLoggerFactory(loggerFactory)
                .WithSender(
                    new HttpSender(
                        "http://jaeger:14268/api/traces"))
                .Build();
                
            var tracer = new Tracer.Builder(serviceName)
                .WithSampler(new ConstSampler(true))
                .WithReporter(reporter)
                .Build();
                
            return tracer;
        }
    }
}