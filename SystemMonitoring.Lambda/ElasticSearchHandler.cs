using System;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json;

namespace SystemMonitoring.Lambda
{
    public class ElasticSearchHandler : IUsageInfoHandler
    {
        private readonly IElasticClient _elasticClient;

        public ElasticSearchHandler(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }
        public async Task Store(UsageInfo body)
        {
            var response = await _elasticClient.IndexDocumentAsync(body);
        }
    }
}
