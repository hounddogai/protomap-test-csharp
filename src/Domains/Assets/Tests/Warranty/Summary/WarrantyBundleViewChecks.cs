namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantyBundleViewChecks
{
    public static void WarrantyBundleViewShowsTheStatusInCapitals()
    {
        var item = WarrantyBundle.NewWarrantyBundle("code-798", "title-380");
        var view = WarrantyBundleView.ToWarrantyBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyBundleView.ToWarrantyBundleViews(new List<WarrantyBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyBundleViewShowsTheStatusInCapitals();
    }
}
