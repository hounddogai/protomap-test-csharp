namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyRollupViewChecks
{
    public static void WarrantyRollupViewShowsTheStatusInCapitals()
    {
        var item = WarrantyRollup.NewWarrantyRollup("code-798", "name-446");
        var view = WarrantyRollupView.ToWarrantyRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyRollupView.ToWarrantyRollupViews(new List<WarrantyRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyRollupViewShowsTheStatusInCapitals();
    }
}
