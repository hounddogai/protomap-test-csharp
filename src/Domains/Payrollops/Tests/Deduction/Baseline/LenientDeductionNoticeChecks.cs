namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LenientDeductionNoticeChecks
{
    public static void LenientDeductionNoticeStartsInIdle()
    {
        var item = LenientDeductionNotice.NewLenientDeductionNotice("code-779", "label-697");
        Check.Equal(item.Status, LenientDeductionNoticeStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientDeductionNoticeMovesToReady()
    {
        var item = LenientDeductionNotice.NewLenientDeductionNotice("code-779", "label-697");
        try
        {
            item.MoveTo(LenientDeductionNoticeStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientDeductionNoticeStatus.Ready, "status");
    }

    public static void LenientDeductionNoticeRejectsMovesFromStopped()
    {
        var item = LenientDeductionNotice.NewLenientDeductionNotice("code-779", "label-697");
        item.Status = LenientDeductionNoticeStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientDeductionNoticeStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientDeductionNoticeStartsInIdle();
        LenientDeductionNoticeMovesToReady();
        LenientDeductionNoticeRejectsMovesFromStopped();
    }
}
