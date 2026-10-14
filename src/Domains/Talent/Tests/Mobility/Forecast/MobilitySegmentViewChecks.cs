namespace Acme.Workforce.Talent.Tests.Mobility.Forecast;

using Acme.Workforce.Talent.Mobility.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class MobilitySegmentViewChecks
{
    public static void MobilitySegmentViewShowsTheStatusInCapitals()
    {
        var item = MobilitySegment.NewMobilitySegment("code-666", "name-811");
        var view = MobilitySegmentView.ToMobilitySegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilitySegmentView.ToMobilitySegmentViews(new List<MobilitySegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilitySegmentViewShowsTheStatusInCapitals();
    }
}
