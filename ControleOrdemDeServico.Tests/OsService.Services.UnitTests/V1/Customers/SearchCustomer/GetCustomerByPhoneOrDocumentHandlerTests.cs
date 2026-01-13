using Moq;
using OsService.Domain.Entities;
using OsService.Infrastructure.Repository;
using OsService.Services.V1.Customers.SearchCustomer;

namespace OsService.Services.UnitTests.V1.Customers.Search;

public class GetCustomerByPhoneOrDocumentHandlerTests
{
    private readonly Mock<ICustomerRepository> _customers;
    private readonly GetCustomerByPhoneOrDocumentHandler _sut;

    public GetCustomerByPhoneOrDocumentHandlerTests()
    {
        _customers = new Mock<ICustomerRepository>();
        _sut = new GetCustomerByPhoneOrDocumentHandler(_customers.Object);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, "")]
    [InlineData(null, "   ")]
    public async Task Handle_ShouldThrowArgumentException_WhenBothPhoneAndDocumentMissing(
        string? phone,
        string? document)
    {
        // arrange
        var query = new GetCustomerByPhoneOrDocumentQuery(phone, document);

        // act
        var act = async () => await _sut.Handle(query, CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ArgumentException>(act);

        _customers.Verify(r =>
                r.GetByPhoneOrDocumentAsync(
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCustomerIsNotFound()
    {
        // arrange
        var query = new GetCustomerByPhoneOrDocumentQuery(
            Phone: "5513999999999",
            Document: "12345678900");

        _customers
            .Setup(r => r.GetByPhoneOrDocumentAsync(
                "5513999999999",
                "12345678900",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        // act
        var result = await _sut.Handle(query, CancellationToken.None);

        // assert
        Assert.Null(result);

        _customers.Verify(r => r.GetByPhoneOrDocumentAsync(
                "5513999999999",
                "12345678900",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenCustomerIsFound()
    {
        // arrange
        var entity = new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Phone = "5513999999999",
            Email = "john.doe@example.com",
            Document = "12345678900",
            CreatedAt = DateTime.UtcNow
        };

        var query = new GetCustomerByPhoneOrDocumentQuery(
            Phone: " 5513999999999 ",
            Document: " 12345678900 ");

        _customers
            .Setup(r => r.GetByPhoneOrDocumentAsync(
                "5513999999999",
                "12345678900",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // act
        var result = await _sut.Handle(query, CancellationToken.None);

        // assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result!.Id);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Phone, result.Phone);
        Assert.Equal(entity.Email, result.Email);
        Assert.Equal(entity.Document, result.Document);

        _customers.Verify(r => r.GetByPhoneOrDocumentAsync(
                "5513999999999",
                "12345678900",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}