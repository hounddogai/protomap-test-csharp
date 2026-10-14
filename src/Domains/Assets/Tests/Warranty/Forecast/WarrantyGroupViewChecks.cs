namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyGroupViewChecks
{
    public static void WarrantyGroupViewShowsTheStatusInCapitals()
    {
        var item = WarrantyGroup.NewWarrantyGroup("code-824", "name-685");
        var view = WarrantyGroupView.ToWarrantyGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantyGroupView.ToWarrantyGroupViews(new List<WarrantyGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantyGroupViewShowsTheStatusInCapitals();
    }
}
