namespace RetailBank.ExceptionHandlers;

public static class Bootstrapper
{
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        services.AddProblemDetails();

        services
            .AddExceptionHandler<BadHttpRequestExceptionHandler>()
            .AddExceptionHandler<ValidationExceptionHandler>()
            .AddExceptionHandler<UserExceptionHandler>()
            .AddExceptionHandler<ExceptionHandler>();

        return services;
    }
}
