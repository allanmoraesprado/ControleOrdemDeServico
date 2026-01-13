using Moq;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.ServiceOrders.GetServiceOrderById;

namespace OsService.Services.UnitTests.V1.ServiceOrders.GetById;

public class GetServiceOrderByIdHandlerTests
{
    private readonly Mock<IServiceOrderRepository> _serviceOrders;
    private readonly GetServiceOrderByIdHandler _sut;

    public GetServiceOrderByIdHandlerTests()
    {
        _serviceOrders = new Mock<IServiceOrderRepository>();
        _sut = new GetServiceOrderByIdHandler(_serviceOrders.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenServiceOrderDoesNotExist()
    {
        // arrange
        var id = Guid.NewGuid();
        var query = new GetServiceOrderByIdQuery(id);

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ServiceOrderEntity?)null);

        // act
        var result = await _sut.Handle(query, CancellationToken.None);

        // assert
        Assert.Null(result);
        _serviceOrders.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnServiceOrder_WhenFound()
    {
        // arrange
        var id = Guid.NewGuid();

        var entity = new ServiceOrderEntity
        {
            Id = id,
            Number = 123,
            CustomerId = Guid.NewGuid(),
            Description = "Any description",
            Price = 50m
        };

        var query = new GetServiceOrderByIdQuery(id);

        _serviceOrders
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // act
        var result = await _sut.Handle(query, CancellationToken.None);

        // assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Number, result.Number);
        Assert.Equal(entity.CustomerId, result.CustomerId);

        _serviceOrders.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}