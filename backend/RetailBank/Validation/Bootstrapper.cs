using FluentValidation;
using RetailBank.Models.Dtos;

namespace RetailBank.Validation;

public static class Bootstrapper
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        return services
            .AddScoped<IValidator<StartSimulationRequest>, StartSimulationRequestValidator>()
            .AddScoped<IValidator<CreateTransferRequest>, CreateTransferRequestValidator>()
            .AddScoped<IValidator<CreateLoanAccountRequest>, CreateLoanAccountRequestValidator>();
    }
}
