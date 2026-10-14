namespace Acme.Workforce.Compensation.Tests.Adjustment.Allocation;

using Acme.Workforce.Compensation.Adjustment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class AdjustmentBaselineChecks
{
    public static void AdjustmentBaselineStartsInPlanned()
    {
        var item = AdjustmentBaseline.NewAdjustmentBaseline("code-910", "title-989");
        Check.Equal(item.Status, AdjustmentBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentBaselineMovesToScheduled()
    {
        var item = AdjustmentBaseline.NewAdjustmentBaseline("code-910", "title-989");
        try
        {
            item.MoveTo(AdjustmentBaselineStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentBaselineStatus.Scheduled, "status");
    }

    public static void AdjustmentBaselineRejectsMovesFromCancelled()
    {
        var item = AdjustmentBaseline.NewAdjustmentBaseline("code-910", "title-989");
        item.Status = AdjustmentBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdjustmentBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdjustmentBaselineStartsInPlanned();
        AdjustmentBaselineMovesToScheduled();
        AdjustmentBaselineRejectsMovesFromCancelled();
    }
}
