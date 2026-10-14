namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopSummaryViewChecks
{
    public static void LaptopSummaryViewShowsTheStatusInCapitals()
    {
        var item = LaptopSummary.NewLaptopSummary("code-336", "title-906");
        var view = LaptopSummaryView.ToLaptopSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopSummaryView.ToLaptopSummaryViews(new List<LaptopSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopSummaryViewShowsTheStatusInCapitals();
    }
}
