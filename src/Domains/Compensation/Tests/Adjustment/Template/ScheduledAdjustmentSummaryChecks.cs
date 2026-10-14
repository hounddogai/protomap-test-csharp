namespace Acme.Workforce.Compensation.Tests.Adjustment.Template;

using Acme.Workforce.Compensation.Adjustment.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledAdjustmentSummaryChecks
{
    public static void ScheduledAdjustmentSummaryStartsInProposed()
    {
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("code-664", "label-280");
        Check.Equal(item.Status, ScheduledAdjustmentSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAdjustmentSummaryMovesToApproved()
    {
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("code-664", "label-280");
        try
        {
            item.MoveTo(ScheduledAdjustmentSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAdjustmentSummaryStatus.Approved, "status");
    }

    public static void ScheduledAdjustmentSummaryRejectsMovesFromWithdrawn()
    {
        var item = ScheduledAdjustmentSummary.NewScheduledAdjustmentSummary("code-664", "label-280");
        item.Status = ScheduledAdjustmentSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledAdjustmentSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledAdjustmentSummaryStartsInProposed();
        ScheduledAdjustmentSummaryMovesToApproved();
        ScheduledAdjustmentSummaryRejectsMovesFromWithdrawn();
    }
}
