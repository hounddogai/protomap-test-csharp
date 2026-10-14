namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class WeightedRateCycleChecks
{
    public static void WeightedRateCycleStartsInPlanned()
    {
        var item = WeightedRateCycle.NewWeightedRateCycle("code-628", "name-265");
        Check.Equal(item.Status, WeightedRateCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedRateCycleMovesToScheduled()
    {
        var item = WeightedRateCycle.NewWeightedRateCycle("code-628", "name-265");
        try
        {
            item.MoveTo(WeightedRateCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedRateCycleStatus.Scheduled, "status");
    }

    public static void WeightedRateCycleRejectsMovesFromCancelled()
    {
        var item = WeightedRateCycle.NewWeightedRateCycle("code-628", "name-265");
        item.Status = WeightedRateCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedRateCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedRateCycleStartsInPlanned();
        WeightedRateCycleMovesToScheduled();
        WeightedRateCycleRejectsMovesFromCancelled();
    }
}
