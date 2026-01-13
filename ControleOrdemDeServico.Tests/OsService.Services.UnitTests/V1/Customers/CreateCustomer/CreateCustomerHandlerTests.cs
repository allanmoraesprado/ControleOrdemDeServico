using Microsoft.Extensions.Logging;
using Moq;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.Customers.CreateCustomer;

namespace OsService.Services.UnitTests.V1.Customers.CreateCustomer;

public class CreateCustomerHandlerTests
{
    private readonly Mock<ICustomerRepository> _customers;
    private readonly Mock<ILogger<CreateCustomerHandler>> _logger;
    private readonly CreateCustomerHandler _sut;

    public CreateCustomerHandlerTests()
    {
        _customers = new Mock<ICustomerRepository>();
        _logger = new Mock<ILogger<CreateCustomerHandler>>();

        _sut = new CreateCustomerHandler(
            _customers.Object,
            _logger.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldThrowValidation_WhenNameIsInvalid(string? name)
    {
        // arrange
        var cmd = new CreateCustomerCommand(
            Name: name!,
            Phone: "+5511999999999",
            Email: "test@example.com",
            Document: "12345678900");

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);

        _customers.Verify(r => r.InsertAsync(
                It.IsAny<CustomerEntity>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateCustomer_WhenRequestIsValid()
    {
        // arrange
        var cmd = new CreateCustomerCommand(
            Name: "John Doe",
            Phone: "+5511999999999",
            Email: "john.doe@example.com",
            Document: "12345678900");

        _customers
            .Setup(r => r.InsertAsync(
                It.IsAny<CustomerEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        var resultId = await _sut.Handle(cmd, CancellationToken.None);

        // assert
        _customers.Verify(r => r.InsertAsync(
                It.Is<CustomerEntity>(c =>
                    c.Name == cmd.Name &&
                    c.Phone == cmd.Phone &&
                    c.Email == cmd.Email &&
                    c.Document == cmd.Document),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.NotEqual(Guid.Empty, resultId);
    }
}