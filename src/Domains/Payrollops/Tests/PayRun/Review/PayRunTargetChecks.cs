namespace Acme.Workforce.Payrollops.Tests.PayRun.Review;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PayRunTargetChecks
{
    public static void PayRunTargetStartsInPlanned()
    {
        var item = PayRunTarget.NewPayRunTarget("code-591", "label-31");
        Check.Equal(item.Status, PayRunTargetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunTargetMovesToScheduled()
    {
        var item = PayRunTarget.NewPayRunTarget("code-591", "label-31");
        try
        {
            item.MoveTo(PayRunTargetStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunTargetStatus.Scheduled, "status");
    }

    public static void PayRunTargetRejectsMovesFromCancelled()
    {
        var item = PayRunTarget.NewPayRunTarget("code-591", "label-31");
        item.Status = PayRunTargetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PayRunTargetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PayRunTargetStartsInPlanned();
        PayRunTargetMovesToScheduled();
        PayRunTargetRejectsMovesFromCancelled();
    }
}
