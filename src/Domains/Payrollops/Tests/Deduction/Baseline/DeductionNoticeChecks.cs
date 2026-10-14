namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionNoticeChecks
{
    public static void DeductionNoticeStartsInProposed()
    {
        var item = DeductionNotice.NewDeductionNotice("code-369", "label-395");
        Check.Equal(item.Status, DeductionNoticeStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionNoticeMovesToWithdrawn()
    {
        var item = DeductionNotice.NewDeductionNotice("code-369", "label-395");
        try
        {
            item.MoveTo(DeductionNoticeStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionNoticeStatus.Withdrawn, "status");
    }

    public static void DeductionNoticeRejectsMovesFromWithdrawn()
    {
        var item = DeductionNotice.NewDeductionNotice("code-369", "label-395");
        item.Status = DeductionNoticeStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DeductionNoticeStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DeductionNoticeStartsInProposed();
        DeductionNoticeMovesToWithdrawn();
        DeductionNoticeRejectsMovesFromWithdrawn();
    }
}
