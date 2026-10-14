namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSummaryViewChecks
{
    public static void LicenseSeatSummaryViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatSummary.NewLicenseSeatSummary("code-189", "title-520");
        var view = LicenseSeatSummaryView.ToLicenseSeatSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatSummaryView.ToLicenseSeatSummaryViews(new List<LicenseSeatSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatSummaryViewShowsTheStatusInCapitals();
    }
}
