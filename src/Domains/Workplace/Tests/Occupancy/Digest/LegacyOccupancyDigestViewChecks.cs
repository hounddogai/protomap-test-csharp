namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LegacyOccupancyDigestViewChecks
{
    public static void LegacyOccupancyDigestViewShowsTheStatusInCapitals()
    {
        var item = LegacyOccupancyDigest.NewLegacyOccupancyDigest("code-950", "name-756");
        var view = LegacyOccupancyDigestView.ToLegacyOccupancyDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyOccupancyDigestView.ToLegacyOccupancyDigestViews(new List<LegacyOccupancyDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyOccupancyDigestViewShowsTheStatusInCapitals();
    }
}
