using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services;

public class UriComposer : IUriComposer
{
    public const string CatalogBaseUrlPlaceholder = "http://catalogbaseurltobereplaced";

    private readonly CatalogSettings _catalogSettings;

    public UriComposer(CatalogSettings catalogSettings) => _catalogSettings = catalogSettings;

    public string ComposePicUri(string uriTemplate)
    {
        return uriTemplate.Replace(CatalogBaseUrlPlaceholder, _catalogSettings.CatalogBaseUrl);
    }
}
