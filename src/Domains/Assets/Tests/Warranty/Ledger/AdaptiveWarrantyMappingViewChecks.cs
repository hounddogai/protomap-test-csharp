namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMappingViewChecks
{
    public static void AdaptiveWarrantyMappingViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("code-248", "label-211");
        var view = AdaptiveWarrantyMappingView.ToAdaptiveWarrantyMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveWarrantyMappingView.ToAdaptiveWarrantyMappingViews(new List<AdaptiveWarrantyMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMappingViewShowsTheStatusInCapitals();
    }
}
