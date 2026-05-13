using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

public class CatalogFilterPaginatedSpecification : Specification<CatalogItem>
{
    private const int UnlimitedItems = int.MaxValue;

    public CatalogFilterPaginatedSpecification(int skip, int take, int? brandId, int? typeId)
        : base()
    {
        if (take == 0)
        {
            take = UnlimitedItems;
        }
        Query
            .Where(i => (!brandId.HasValue || i.CatalogBrandId == brandId) &&
            (!typeId.HasValue || i.CatalogTypeId == typeId))
            .Skip(skip).Take(take);
    }
}
