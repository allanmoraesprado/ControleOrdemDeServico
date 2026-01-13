using Microsoft.Extensions.Logging;
using Moq;
using OsService.Domain.Entities;
using OsService.Domain.Enums;
using OsService.Infrastructure.Repository;
using OsService.Services.Exceptions;
using OsService.Services.V1.ServiceOrders.ChangeStatus;

namespace OsService.Services.UnitTests.V1.ServiceOrders.ChangeStatus;

public class ChangeServiceOrderStatusHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _serviceOrders;
    private readonly Mock<ILogger<ChangeServiceOrderStatusHandler>> _logger;
    private readonly ChangeServiceOrderStatusHandler _sut;

    public ChangeServiceOrderStatusHandlerTests()
    {
        _serviceOrders = new Mock<IServiceOrderRepository>();
        _logger = new Mock<ILogger<ChangeServiceOrderStatusHandler>>();

        _sut = new ChangeServiceOrderStatusHandler(
            _serviceOrders.Object,
            _logger.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenServiceOrderDoesNotExist()
    {
        // arrange
        var id = Guid.NewGuid();
        var cmd = new ChangeServiceOrderStatusCommand(
            Id: id,
            NewStatus: ServiceOrderStatus.InProgress);

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceOrderEntity?)null);

        // act
        var act = async () => await _sut.Handle(cmd, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<KeyNotFoundException>(act);

        _serviceOrders.Verify(r => r.UpdateStatusAsync(
                It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(ServiceOrderStatus.Finished)]
    public async Task Handle_ShouldThrowException_WhenChangingFromFinalStatus(ServiceOrderStatus currentStatus)
    {
        // arrange
        var id = Guid.NewGuid();
        var cmd = new ChangeServiceOrderStatusCommand(
            Id: id,
            NewStatus: ServiceOrderStatus.Open);

        var existing = new ServiceOrderEntity
        {
            Id = id,
            Status = currentStatus,
            Price = 100m
        };

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // act
        var ex = await Assert.ThrowsAsync<ConflictException>(
            async () => await _sut.Handle(cmd, CancellationToken.None));

        // assert
        Assert.True(
            ex is not null,
            $"Unexpected exception type: {ex.GetType().FullName}");

        _serviceOrders.Verify(r => r.UpdateStatusAsync(
                It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldChangeStatusFromOpenToInProgress()
    {
        // arrange
        var id = Guid.NewGuid();
        var cmd = new ChangeServiceOrderStatusCommand(
            Id: id,
            NewStatus: ServiceOrderStatus.InProgress);

        var existing = new ServiceOrderEntity
        {
            Id = id,
            Status = ServiceOrderStatus.Open,
            StartedAt = null,
            FinishedAt = null,
            Price = 50m
        };

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _serviceOrders
            .Setup(r => r.UpdateStatusAsync(
                It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        _ = await _sut.Handle(cmd, CancellationToken.None);

        // assert
        _serviceOrders.Verify(r => r.UpdateStatusAsync(
                id,
                ServiceOrderStatus.InProgress,
                It.Is<DateTime?>(d => d.HasValue),
                It.Is<DateTime?>(d => d == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldChangeStatusFromInProgressToFinished_WhenPriceIsFilled()
    {
        // arrange
        var id = Guid.NewGuid();
        var startedAt = DateTime.UtcNow.AddMinutes(-5);

        var cmd = new ChangeServiceOrderStatusCommand(
            Id: id,
            NewStatus: ServiceOrderStatus.Finished);

        var existing = new ServiceOrderEntity
        {
            Id = id,
            Status = ServiceOrderStatus.InProgress,
            StartedAt = startedAt,
            FinishedAt = null,
            Price = 150m
        };

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _serviceOrders
            .Setup(r => r.UpdateStatusAsync(
                It.IsAny<Guid>(),
                It.IsAny<ServiceOrderStatus>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // act
        _ = await _sut.Handle(cmd, CancellationToken.None);

        // assert
        _serviceOrders.Verify(r => r.UpdateStatusAsync(
                id,
                ServiceOrderStatus.Finished,
                It.Is<DateTime?>(d => d == startedAt),
                It.Is<DateTime?>(d => d.HasValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}