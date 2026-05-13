namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Resolves catalog image URI templates to absolute URLs for the current environment.</summary>
public interface IUriComposer
{
    /// <summary>Replaces the base-URL placeholder in a stored URI template with the configured catalog base URL.</summary>
    string ComposePicUri(string uriTemplate);
}
