namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSummaryChecks
{
    public static void AdjustmentSummaryStartsInIdle()
    {
        var item = AdjustmentSummary.NewAdjustmentSummary("code-90", "label-148");
        Check.Equal(item.Status, AdjustmentSummaryStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentSummaryMovesToReady()
    {
        var item = AdjustmentSummary.NewAdjustmentSummary("code-90", "label-148");
        try
        {
            item.MoveTo(AdjustmentSummaryStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentSummaryStatus.Ready, "status");
    }

    public static void AdjustmentSummaryRejectsMovesFromStopped()
    {
        var item = AdjustmentSummary.NewAdjustmentSummary("code-90", "label-148");
        item.Status = AdjustmentSummaryStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdjustmentSummaryStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdjustmentSummaryStartsInIdle();
        AdjustmentSummaryMovesToReady();
        AdjustmentSummaryRejectsMovesFromStopped();
    }
}
