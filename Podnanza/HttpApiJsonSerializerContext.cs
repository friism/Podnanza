using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json.Serialization;

namespace Podnanza
{
    [JsonSerializable(typeof(APIGatewayHttpApiV2ProxyRequest))]
    [JsonSerializable(typeof(APIGatewayHttpApiV2ProxyResponse))]
    public partial class HttpApiJsonSerializerContext : JsonSerializerContext
    {
    }
}
