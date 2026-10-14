namespace Acme.Workforce.Assets.Tests.LicenseSeat.RecordItem;

using Acme.Workforce.Assets.LicenseSeat.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LicenseSeatSegmentViewChecks
{
    public static void LicenseSeatSegmentViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatSegment.NewLicenseSeatSegment("code-431", "label-701");
        var view = LicenseSeatSegmentView.ToLicenseSeatSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatSegmentView.ToLicenseSeatSegmentViews(new List<LicenseSeatSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatSegmentViewShowsTheStatusInCapitals();
    }
}
