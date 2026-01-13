using Moq;
using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.ChangePrice;

namespace OsService.Services.UnitTests.V1.ServiceOrders.ChangePrice;

public class ChangeServiceOrderPriceHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _serviceOrders;
    private readonly ChangeServiceOrderPriceHandler _sut;

    public ChangeServiceOrderPriceHandlerTests()
    {
        _serviceOrders = new Mock<IServiceOrderRepository>();
        _sut = new ChangeServiceOrderPriceHandler(_serviceOrders.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowValidation_WhenPriceIsNegative()
    {
        // arrange
        var cmd = new ChangeServiceOrderPriceCommand(
            Id: Guid.NewGuid(),
            Price: -1m);

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);

        _serviceOrders.Verify(r => r.UpdatePriceAsync(
                It.IsAny<Guid>(),
                It.IsAny<decimal?>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenServiceOrderDoesNotExist()
    {
        // arrange
        var id = Guid.NewGuid();
        var cmd = new ChangeServiceOrderPriceCommand(
            Id: id,
            Price: 100m);

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceOrderEntity?)null);

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _serviceOrders.Verify(r => r.UpdatePriceAsync(
                It.IsAny<Guid>(),
                It.IsAny<decimal?>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.Finished)]
    public async Task Handle_ShouldThrowConflict_WhenStatusIsFinal(ServiceOrderStatus currentStatus)
    {
        // arrange
        var id = Guid.NewGuid();
        var cmd = new ChangeServiceOrderPriceCommand(
            Id: id,
            Price: 100m);

        var existing = new ServiceOrderEntity
        {
            Id = id,
            Status = currentStatus,
            Price = 100m,
            Coin = "BRL"
        };

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ConflictException>(act);

        _serviceOrders.Verify(r => r.UpdatePriceAsync(
                It.IsAny<Guid>(),
                It.IsAny<decimal?>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.Open)]
    [InlineData(ServiceOrderStatus.InProgress)]
    public async Task Handle_ShouldUpdatePrice_WhenStatusAllowsChange(ServiceOrderStatus currentStatus)
    {
        // arrange
        var id = Guid.NewGuid();
        var newPrice = 250m;

        var cmd = new ChangeServiceOrderPriceCommand(
            Id: id,
            Price: newPrice);

        var existing = new ServiceOrderEntity
        {
            Id = id,
            Status = currentStatus,
            Price = 100m,
            Coin = "BRL"
        };

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _serviceOrders
            .Setup(r => r.UpdatePriceAsync(
                It.IsAny<Guid>(),
                It.IsAny<decimal?>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        _ = await _sut.Handle(cmd, CancellationToken.None);

        // assert
        _serviceOrders.Verify(r => r.UpdatePriceAsync(
                id,
                newPrice,
                existing.Coin,
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}