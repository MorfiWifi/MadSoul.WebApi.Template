using MadSoul.AspCommon;
using MadSoul.Common;

namespace Products.HttpServer;

public  class SampleEndpoint(IApi1Configuration apiConfiguration) : IEndpointRoute
{
    public void RegisterRoute(IEndpointRouteBuilder app)
    {
        var baseUrl = apiConfiguration.BaseUrl;
        app.MapGet($"{baseUrl}/name", () => "this is api 1").WithOpenApi();
    }
}