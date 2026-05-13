using System.Collections.Generic;
using System.Linq;
using Ardalis.Result;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.BasketServiceTests;

public class SetQuantities
{
    private readonly IRepository<Basket> _mockBasketRepo = Substitute.For<IRepository<Basket>>();
    private readonly IAppLogger<BasketService> _mockLogger = Substitute.For<IAppLogger<BasketService>>();

    [Fact]
    public async Task ReturnsNotFoundWhenBasketDoesNotExist()
    {
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Basket?)null);

        var basketService = new BasketService(_mockBasketRepo, _mockLogger);
        var result = await basketService.SetQuantities(99, new Dictionary<string, int>());

        Assert.Equal(ResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task UpdatesQuantityForSingleItem()
    {
        var basket = new Basket("buyer@test.com");
        basket.AddItem(1, 10m, 3);

        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);

        var basketService = new BasketService(_mockBasketRepo, _mockLogger);
        var itemId = basket.Items.First().Id.ToString();
        var quantities = new Dictionary<string, int> { { itemId, 7 } };

        var result = await basketService.SetQuantities(basket.Id, quantities);

        Assert.True(result.IsSuccess);
        Assert.Equal(7, basket.Items.First().Quantity);
    }

    [Fact]
    public async Task RemovesItemSetToZeroQuantity()
    {
        var basket = new Basket("buyer@test.com");
        basket.AddItem(1, 10m, 3);

        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);

        var basketService = new BasketService(_mockBasketRepo, _mockLogger);
        var itemId = basket.Items.First().Id.ToString();
        var quantities = new Dictionary<string, int> { { itemId, 0 } };

        var result = await basketService.SetQuantities(basket.Id, quantities);

        Assert.True(result.IsSuccess);
        Assert.Empty(basket.Items);
    }

    [Fact]
    public async Task InvokesRepositoryUpdateAsync()
    {
        var basket = new Basket("buyer@test.com");
        basket.AddItem(1, 10m, 1);

        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);

        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        await basketService.SetQuantities(basket.Id, new Dictionary<string, int>());

        await _mockBasketRepo.Received().UpdateAsync(basket, Arg.Any<CancellationToken>());
    }
}
