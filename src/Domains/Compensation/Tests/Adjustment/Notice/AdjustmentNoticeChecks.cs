namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentNoticeChecks
{
    public static void AdjustmentNoticeStartsInNew()
    {
        var item = AdjustmentNotice.NewAdjustmentNotice("code-393", "label-19");
        Check.Equal(item.Status, AdjustmentNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentNoticeMovesToDone()
    {
        var item = AdjustmentNotice.NewAdjustmentNotice("code-393", "label-19");
        try
        {
            item.MoveTo(AdjustmentNoticeStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentNoticeStatus.Done, "status");
    }

    public static void AdjustmentNoticeRejectsMovesFromFailed()
    {
        var item = AdjustmentNotice.NewAdjustmentNotice("code-393", "label-19");
        item.Status = AdjustmentNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(AdjustmentNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        AdjustmentNoticeStartsInNew();
        AdjustmentNoticeMovesToDone();
        AdjustmentNoticeRejectsMovesFromFailed();
    }
}
