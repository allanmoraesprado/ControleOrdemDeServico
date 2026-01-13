using Microsoft.Extensions.Logging;
using Moq;
using OsService.Domain.Entities;
using OsService.Services.Exceptions;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.ServiceOrders.OpenServiceOrder;

namespace OsService.Services.UnitTests.V1.ServiceOrders.OpenServiceOrder;

public class OpenServiceOrderHandlerTests
{
    private readonly Mock<ICustomerRepository> _customers;
    private readonly Mock<IServiceOrderRepository> _serviceOrders;
    private readonly Mock<ILogger<OpenServiceOrderHandler>> _logger;
    private readonly OpenServiceOrderHandler _sut;

    public OpenServiceOrderHandlerTests()
    {
        _customers = new Mock<ICustomerRepository>();
        _serviceOrders = new Mock<IServiceOrderRepository>();
        _logger = new Mock<ILogger<OpenServiceOrderHandler>>();

        _sut = new OpenServiceOrderHandler(
            _customers.Object,
            _serviceOrders.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCustomerIdIsEmpty()
    {
        // arrange
        var cmd = new OpenServiceOrderCommand(
            CustomerId: Guid.Empty,
            Description: "Any description",
            Price: 10m
        );

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);
        _serviceOrders.Verify(r => r.InsertAndReturnNumberAsync(It.IsAny<ServiceOrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldThrow_WhenDescriptionIsNullOrWhiteSpace(string? description)
    {
        // arrange
        var cmd = new OpenServiceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Description: description!,
            Price: 10m
        );

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);
        _serviceOrders.Verify(r => r.InsertAndReturnNumberAsync(It.IsAny<ServiceOrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenDescriptionIsTooLong()
    {
        // arrange
        var longDescription = new string('x', 501);
        var cmd = new OpenServiceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Description: longDescription,
            Price: 10m
        );

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);
        _serviceOrders.Verify(r => r.InsertAndReturnNumberAsync(It.IsAny<ServiceOrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPriceIsNegative()
    {
        // arrange
        var cmd = new OpenServiceOrderCommand(
            CustomerId: Guid.NewGuid(),
            Description: "Valid description",
            Price: -1m
        );

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);
        _serviceOrders.Verify(r => r.InsertAndReturnNumberAsync(It.IsAny<ServiceOrderEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateServiceOrder_WhenRequestIsValid()
    {
        // arrange
        var customerId = Guid.NewGuid();
        var cmd = new OpenServiceOrderCommand(
            CustomerId: customerId,
            Description: "Valid description",
            Price: 100m
        );

        _customers
            .Setup(x => x.ExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _serviceOrders
            .Setup(x => x.InsertAndReturnNumberAsync(
                It.IsAny<ServiceOrderEntity>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid.NewGuid(), 123));

        // act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // assert
        _customers.Verify(x => x.ExistsAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceOrders.Verify(x => x.InsertAndReturnNumberAsync(It.IsAny<ServiceOrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.Number > 0);
    }
}