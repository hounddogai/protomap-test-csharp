namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantySegmentViewChecks
{
    public static void WarrantySegmentViewShowsTheStatusInCapitals()
    {
        var item = WarrantySegment.NewWarrantySegment("code-584", "label-712");
        var view = WarrantySegmentView.ToWarrantySegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WarrantySegmentView.ToWarrantySegmentViews(new List<WarrantySegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WarrantySegmentViewShowsTheStatusInCapitals();
    }
}
