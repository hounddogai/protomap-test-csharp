namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultLicenseSeatSummaryViewChecks
{
    public static void DefaultLicenseSeatSummaryViewShowsTheStatusInCapitals()
    {
        var item = DefaultLicenseSeatSummary.NewDefaultLicenseSeatSummary("code-50", "title-305");
        var view = DefaultLicenseSeatSummaryView.ToDefaultLicenseSeatSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultLicenseSeatSummaryView.ToDefaultLicenseSeatSummaryViews(new List<DefaultLicenseSeatSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultLicenseSeatSummaryViewShowsTheStatusInCapitals();
    }
}
