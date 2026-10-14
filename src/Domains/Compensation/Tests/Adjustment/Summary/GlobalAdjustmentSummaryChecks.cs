namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalAdjustmentSummaryChecks
{
    public static void GlobalAdjustmentSummaryStartsInRequested()
    {
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("code-867", "title-524");
        Check.Equal(item.Status, GlobalAdjustmentSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAdjustmentSummaryMovesToExpired()
    {
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("code-867", "title-524");
        try
        {
            item.MoveTo(GlobalAdjustmentSummaryStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAdjustmentSummaryStatus.Expired, "status");
    }

    public static void GlobalAdjustmentSummaryRejectsMovesFromExpired()
    {
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("code-867", "title-524");
        item.Status = GlobalAdjustmentSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalAdjustmentSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalAdjustmentSummaryStartsInRequested();
        GlobalAdjustmentSummaryMovesToExpired();
        GlobalAdjustmentSummaryRejectsMovesFromExpired();
    }
}
