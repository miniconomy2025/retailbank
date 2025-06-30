namespace RetailBank.Endpoints;

public static class Bootstrapper
{
    public static IEndpointRouteBuilder AddEndpoints(this IEndpointRouteBuilder routes)
    {
        return routes
            .AddAccountEndpoints()
            .AddLoanEndpoints()
            .AddTransferEndpoints()
            .AddSimulationEndpoints();
    }
}
