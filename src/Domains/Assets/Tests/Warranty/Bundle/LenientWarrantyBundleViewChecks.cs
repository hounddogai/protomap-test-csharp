namespace Acme.Workforce.Assets.Tests.Warranty.Bundle;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class LenientWarrantyBundleViewChecks
{
    public static void LenientWarrantyBundleViewShowsTheStatusInCapitals()
    {
        var item = LenientWarrantyBundle.NewLenientWarrantyBundle("code-551", "label-666");
        var view = LenientWarrantyBundleView.ToLenientWarrantyBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientWarrantyBundleView.ToLenientWarrantyBundleViews(new List<LenientWarrantyBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientWarrantyBundleViewShowsTheStatusInCapitals();
    }
}
