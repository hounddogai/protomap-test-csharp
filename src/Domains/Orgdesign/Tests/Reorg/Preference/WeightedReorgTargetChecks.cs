namespace Acme.Workforce.Orgdesign.Tests.Reorg.Preference;

using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedReorgTargetChecks
{
    public static void WeightedReorgTargetStartsInPlanned()
    {
        var item = WeightedReorgTarget.NewWeightedReorgTarget("code-324", "label-262");
        Check.Equal(item.Status, WeightedReorgTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedReorgTargetMovesToScheduled()
    {
        var item = WeightedReorgTarget.NewWeightedReorgTarget("code-324", "label-262");
        try
        {
            item.MoveTo(WeightedReorgTargetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedReorgTargetStatus.Scheduled, "status");
    }

    public static void WeightedReorgTargetRejectsMovesFromCancelled()
    {
        var item = WeightedReorgTarget.NewWeightedReorgTarget("code-324", "label-262");
        item.Status = WeightedReorgTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedReorgTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedReorgTargetStartsInPlanned();
        WeightedReorgTargetMovesToScheduled();
        WeightedReorgTargetRejectsMovesFromCancelled();
    }
}
