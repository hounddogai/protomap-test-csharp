namespace Acme.Workforce.Payrollops.Tests.Deduction.Review;

using Acme.Workforce.Payrollops.Deduction.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualDeductionNoticeChecks
{
    public static void ManualDeductionNoticeStartsInNew()
    {
        var item = ManualDeductionNotice.NewManualDeductionNotice("code-503", "title-976");
        Check.Equal(item.Status, ManualDeductionNoticeStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualDeductionNoticeMovesToRunning()
    {
        var item = ManualDeductionNotice.NewManualDeductionNotice("code-503", "title-976");
        try
        {
            item.MoveTo(ManualDeductionNoticeStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualDeductionNoticeStatus.Running, "status");
    }

    public static void ManualDeductionNoticeRejectsMovesFromFailed()
    {
        var item = ManualDeductionNotice.NewManualDeductionNotice("code-503", "title-976");
        item.Status = ManualDeductionNoticeStatus.Failed;
        Check.Fails(() => item.MoveTo(ManualDeductionNoticeStatus.New), "move");
    }

    public static void RunAll()
    {
        ManualDeductionNoticeStartsInNew();
        ManualDeductionNoticeMovesToRunning();
        ManualDeductionNoticeRejectsMovesFromFailed();
    }
}
