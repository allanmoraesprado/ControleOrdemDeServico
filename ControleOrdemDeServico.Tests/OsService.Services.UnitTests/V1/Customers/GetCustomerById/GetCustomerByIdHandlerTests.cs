using Microsoft.Extensions.Logging;
using Moq;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.Customers.GetCustomerById;

namespace OsService.Services.UnitTests.V1.Customers.GetById;

public class GetCustomerByIdHandlerTests
{
    private readonly Mock<ICustomerRepository> _customers;
    private readonly GetCustomerByIdHandler _sut;

    public GetCustomerByIdHandlerTests()
    {
        _customers = new Mock<ICustomerRepository>();
        _sut = new GetCustomerByIdHandler(_customers.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerDoesNotExist()
    {
        // arrange
        var id = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(id);

        _customers
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // act
        var result = await _sut.Handle(query, CancellationToken.None);

        // assert
        Assert.Null(result);

        _customers.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCustomer_WhenFound()
    {
        // arrange
        var id = Guid.NewGuid();

        var entity = new CustomerEntity
        {
            Id = id,
            Name = "John Doe",
            Email = "john.doe@example.com",
            Document = "12345678900"
        };

        var query = new GetCustomerByIdQuery(id);

        _customers
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // act
        var result = await _sut.Handle(query, CancellationToken.None);

        // assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Email, result.Email);

        _customers.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }
}