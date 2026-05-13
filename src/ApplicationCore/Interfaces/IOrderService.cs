using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Creates orders from a buyer's basket.</summary>
public interface IOrderService
{
    /// <summary>Converts the basket into a confirmed order with the given shipping address, then empties the basket.</summary>
    Task CreateOrderAsync(int basketId, Address shippingAddress);
}
