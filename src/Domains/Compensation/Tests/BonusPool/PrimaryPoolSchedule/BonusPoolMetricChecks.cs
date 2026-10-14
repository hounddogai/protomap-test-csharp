namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolMetricChecks
{
    public static void BonusPoolMetricStartsInCreated()
    {
        var item = BonusPoolMetric.NewBonusPoolMetric("code-837", "label-775");
        Check.Equal(item.Status, BonusPoolMetricStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolMetricMovesToPublished()
    {
        var item = BonusPoolMetric.NewBonusPoolMetric("code-837", "label-775");
        try
        {
            item.MoveTo(BonusPoolMetricStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolMetricStatus.Published, "status");
    }

    public static void BonusPoolMetricRejectsMovesFromRetired()
    {
        var item = BonusPoolMetric.NewBonusPoolMetric("code-837", "label-775");
        item.Status = BonusPoolMetricStatus.Retired;
        Check.Fails(() => item.MoveTo(BonusPoolMetricStatus.Created), "move");
    }

    public static void RunAll()
    {
        BonusPoolMetricStartsInCreated();
        BonusPoolMetricMovesToPublished();
        BonusPoolMetricRejectsMovesFromRetired();
    }
}
