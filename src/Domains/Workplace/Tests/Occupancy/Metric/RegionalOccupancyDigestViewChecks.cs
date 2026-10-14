namespace Acme.Workforce.Workplace.Tests.Occupancy.Metric;

using Acme.Workforce.Workplace.Occupancy.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class RegionalOccupancyDigestViewChecks
{
    public static void RegionalOccupancyDigestViewShowsTheStatusInCapitals()
    {
        var item = RegionalOccupancyDigest.NewRegionalOccupancyDigest("code-309", "title-521");
        var view = RegionalOccupancyDigestView.ToRegionalOccupancyDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalOccupancyDigestView.ToRegionalOccupancyDigestViews(new List<RegionalOccupancyDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalOccupancyDigestViewShowsTheStatusInCapitals();
    }
}
