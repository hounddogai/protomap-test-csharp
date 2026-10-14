namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentNoticeChecks
{
    public static void AdaptiveAdjustmentNoticeStartsInOpen()
    {
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("code-509", "name-395");
        Check.Equal(item.Status, AdaptiveAdjustmentNoticeStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveAdjustmentNoticeMovesToArchived()
    {
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("code-509", "name-395");
        try
        {
            item.MoveTo(AdaptiveAdjustmentNoticeStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveAdjustmentNoticeStatus.Archived, "status");
    }

    public static void AdaptiveAdjustmentNoticeRejectsMovesFromArchived()
    {
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("code-509", "name-395");
        item.Status = AdaptiveAdjustmentNoticeStatus.Archived;
        Check.Fails(() => item.MoveTo(AdaptiveAdjustmentNoticeStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentNoticeStartsInOpen();
        AdaptiveAdjustmentNoticeMovesToArchived();
        AdaptiveAdjustmentNoticeRejectsMovesFromArchived();
    }
}
