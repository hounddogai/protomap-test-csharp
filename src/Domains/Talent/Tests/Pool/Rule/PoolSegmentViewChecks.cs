namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolSegmentViewChecks
{
    public static void PoolSegmentViewShowsTheStatusInCapitals()
    {
        var item = PoolSegment.NewPoolSegment("code-338", "title-125");
        var view = PoolSegmentView.ToPoolSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolSegmentView.ToPoolSegmentViews(new List<PoolSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolSegmentViewShowsTheStatusInCapitals();
    }
}
