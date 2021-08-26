using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SystemMonitoring.Lambda
{
    public class Function
    {
        public Func<IServiceProvider> ConfigureServices = () =>
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build() as IConfiguration;
            serviceCollection.Configure<AppSettings>(configuration);
            serviceCollection.AddSingleton<IUsageInfoHandler, ElasticSearchHandler>();
            serviceCollection.AddDefaultAWSOptions(configuration.GetAWSOptions());
            var options = configuration.GetAWSOptions();

            serviceCollection.AddSingleton<IElasticClient>(x =>
            {
                var configs= x.GetService<IOptions<AppSettings>>().Value;
                var httpConnection = new AwsHttpConnection(new AWSOptions()
                {
                    Credentials = options.Credentials,
                    Region = options.Region
                });
                var settings = new ConnectionSettings(new SingleNodeConnectionPool(new Uri(configs.ElasticSearchUrl)), httpConnection)
                    .DefaultIndex(configs.Index)
                    .DefaultMappingFor<UsageInfo>(m => m
                        .PropertyName(p => p.Id, "id")
                    );
                return new ElasticClient(settings);
            });

            return serviceCollection.BuildServiceProvider();
        };
        IUsageInfoHandler usageInfoHandler { get; set; }
        public Function()
        {
            var provider = ConfigureServices();
            usageInfoHandler = provider.GetService<IUsageInfoHandler>();
        }


        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
        {
            foreach (var message in evnt.Records)
            {
                Console.WriteLine($"Processing {message.Body}");
                try
                {
                    var info = JsonSerializer.Deserialize<UsageInfo>(message.Body);
                    Console.WriteLine("Message deserialized");
                    await usageInfoHandler.Store(info);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

    }
}
