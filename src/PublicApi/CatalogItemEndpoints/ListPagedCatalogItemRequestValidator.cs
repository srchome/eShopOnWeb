using FluentValidation;

namespace Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;

public class ListPagedCatalogItemRequestValidator : AbstractValidator<ListPagedCatalogItemRequest>
{
    public ListPagedCatalogItemRequestValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Page index must be 0 or greater");
        
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100");
        
        RuleFor(x => x.CatalogBrandId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Brand ID must be 0 or greater");
        
        RuleFor(x => x.CatalogTypeId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Type ID must be 0 or greater");
    }
}
