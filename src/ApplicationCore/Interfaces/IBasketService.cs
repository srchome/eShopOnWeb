using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Manages shopping basket operations including item management and basket lifecycle.</summary>
public interface IBasketService
{
    /// <summary>Copies all items from an anonymous basket into the named user's basket, then deletes the anonymous basket.</summary>
    Task TransferBasketAsync(string anonymousId, string userName);

    /// <summary>Adds a catalog item to the user's basket, creating the basket if it does not already exist.</summary>
    Task<Basket> AddItemToBasket(string username, int catalogItemId, decimal price, int quantity = 1);

    /// <summary>Updates item quantities in the basket; removes any item whose quantity is set to zero.</summary>
    Task<Result<Basket>> SetQuantities(int basketId, Dictionary<string, int> quantities);

    /// <summary>Permanently deletes the basket and all its items.</summary>
    Task DeleteBasketAsync(int basketId);
}
