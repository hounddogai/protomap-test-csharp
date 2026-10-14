namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorTemplateViewChecks
{
    public static void VendorTemplateViewShowsTheStatusInCapitals()
    {
        var item = VendorTemplate.NewVendorTemplate("code-570", "name-325");
        var view = VendorTemplateView.ToVendorTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(VendorTemplateView.ToVendorTemplateViews(new List<VendorTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        VendorTemplateViewShowsTheStatusInCapitals();
    }
}
