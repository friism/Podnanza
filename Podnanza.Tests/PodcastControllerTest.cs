using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Podnanza.Tests
{
    public class PodcastControllerTest
    {
        // TODO: Get this working
        [Fact]
        public async Task TestGet()
        {
            var lambdaFunction = new LambdaEntryPoint();

            var requestStr = File.ReadAllText("./SampleRequests/PodcastController-Get.json");
            var request = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(requestStr);
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.True(response.Headers.ContainsKey("Content-Type"));
            Assert.Equal("application/rss+xml", response.Headers["Content-Type"]);
        }
    }
}
