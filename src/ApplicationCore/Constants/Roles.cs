namespace Microsoft.eShopWeb.ApplicationCore.Constants;

public static class Roles
{
    public const string ADMINISTRATORS = "Administrators";
    public const string PRODUCT_MANAGERS = "Product Managers";
    public const string ADMIN_PORTAL_ROLES = $"{ADMINISTRATORS},{PRODUCT_MANAGERS}";
}
