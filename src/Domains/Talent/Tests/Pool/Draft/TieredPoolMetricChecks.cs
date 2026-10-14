namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class TieredPoolMetricChecks
{
    public static void TieredPoolMetricStartsInCreated()
    {
        var item = TieredPoolMetric.NewTieredPoolMetric("code-683", "label-94");
        Check.Equal(item.Status, TieredPoolMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPoolMetricMovesToPublished()
    {
        var item = TieredPoolMetric.NewTieredPoolMetric("code-683", "label-94");
        try
        {
            item.MoveTo(TieredPoolMetricStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPoolMetricStatus.Published, "status");
    }

    public static void TieredPoolMetricRejectsMovesFromRetired()
    {
        var item = TieredPoolMetric.NewTieredPoolMetric("code-683", "label-94");
        item.Status = TieredPoolMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredPoolMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredPoolMetricStartsInCreated();
        TieredPoolMetricMovesToPublished();
        TieredPoolMetricRejectsMovesFromRetired();
    }
}
