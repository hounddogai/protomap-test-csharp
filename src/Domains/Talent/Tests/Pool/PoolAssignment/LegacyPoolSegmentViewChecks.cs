namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class LegacyPoolSegmentViewChecks
{
    public static void LegacyPoolSegmentViewShowsTheStatusInCapitals()
    {
        var item = LegacyPoolSegment.NewLegacyPoolSegment("code-776", "name-420");
        var view = LegacyPoolSegmentView.ToLegacyPoolSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPoolSegmentView.ToLegacyPoolSegmentViews(new List<LegacyPoolSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPoolSegmentViewShowsTheStatusInCapitals();
    }
}
