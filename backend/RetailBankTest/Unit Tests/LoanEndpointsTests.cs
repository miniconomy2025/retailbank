using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using RetailBank.Endpoints;
using RetailBank.Exceptions;
using RetailBank.Models.Dtos;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using RetailBank.Services;
using RetailBank.Validation;

namespace RetailBankTest;

public class LoanEndpointsTests
{
    private readonly Mock<ILoanService> _mockLoanService;
    private readonly Mock<IValidator<CreateLoanAccountRequest>> _mockValidator;

    public LoanEndpointsTests()
    {
        _mockLoanService = new Mock<ILoanService>();
        _mockValidator = new Mock<IValidator<CreateLoanAccountRequest>>();
    }

    [Fact]
    public async Task CreateLoanAccount_WithValidRequest_ReturnsOkWithAccountId()
    {
        // Arrange
        var request = new CreateLoanAccountRequest(
            LoanAmountCents: 100000ul,
            DebtorAccountId: "100000000001"
        );

        var expectedAccountId = new UInt128(0, 1000123456789);

        _mockValidator
            .Setup(v => v.Validate(It.IsAny<FluentValidation.IValidationContext>()))
            .Returns(new FluentValidation.Results.ValidationResult());

        _mockLoanService
            .Setup(s => s.CreateLoanAccount(
                It.Is<UInt128>(id => id == UInt128.Parse(request.DebtorAccountId)),
                request.LoanAmountCents
            ))
            .ReturnsAsync(expectedAccountId);

        // Act
        var result = await LoanEndpoints.CreateLoanAccount(
            request,
            _mockLoanService.Object,
            _mockValidator.Object
        );

        // Assert
        var okResult = Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<CreateLoanAccountResponse>>(result);
        Assert.Equal(expectedAccountId.ToString(), okResult.Value?.AccountId);
    }

    [Fact]
    public async Task CreateLoanAccount_WithValidationFailure_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateLoanAccountRequest(
            LoanAmountCents: 0ul, // Invalid: zero amount
            DebtorAccountId: "123" // Invalid: wrong length
        );

        var validator = new CreateLoanAccountRequestValidator();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => LoanEndpoints.CreateLoanAccount(
                request,
                _mockLoanService.Object,
                validator
            )
        );

        Assert.NotEmpty(exception.Errors);
    }

    [Fact]
    public async Task CreateLoanAccount_WithServiceException_PropagatesException()
    {
        // Arrange
        var request = new CreateLoanAccountRequest(
            LoanAmountCents: 100000ul,
            DebtorAccountId: "100000000001"
        );

        _mockValidator
            .Setup(v => v.Validate(It.IsAny<FluentValidation.IValidationContext>()))
            .Returns(new FluentValidation.Results.ValidationResult());

        _mockLoanService
            .Setup(s => s.CreateLoanAccount(
                It.Is<UInt128>(id => id == UInt128.Parse(request.DebtorAccountId)),
                request.LoanAmountCents
            ))
            .ThrowsAsync(new AccountNotFoundException(UInt128.Parse(request.DebtorAccountId)));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AccountNotFoundException>(
            () => LoanEndpoints.CreateLoanAccount(
                request,
                _mockLoanService.Object,
                _mockValidator.Object
            )
        );

        Assert.Contains(request.DebtorAccountId, exception.Message);
    }
}
