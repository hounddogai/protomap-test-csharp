namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class StrictAdjustmentBaselineChecks
{
    public static void StrictAdjustmentBaselineStartsInRequested()
    {
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("code-658", "name-803");
        Check.Equal(item.Status, StrictAdjustmentBaselineStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictAdjustmentBaselineMovesToGranted()
    {
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("code-658", "name-803");
        try
        {
            item.MoveTo(StrictAdjustmentBaselineStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictAdjustmentBaselineStatus.Granted, "status");
    }

    public static void StrictAdjustmentBaselineRejectsMovesFromExpired()
    {
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("code-658", "name-803");
        item.Status = StrictAdjustmentBaselineStatus.Expired;
        Check.Fails(() => item.MoveTo(StrictAdjustmentBaselineStatus.Requested), "move");
    }

    public static void RunAll()
    {
        StrictAdjustmentBaselineStartsInRequested();
        StrictAdjustmentBaselineMovesToGranted();
        StrictAdjustmentBaselineRejectsMovesFromExpired();
    }
}
