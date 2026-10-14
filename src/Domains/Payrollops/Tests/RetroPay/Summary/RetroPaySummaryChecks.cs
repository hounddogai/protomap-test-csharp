namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RetroPaySummaryChecks
{
    public static void RetroPaySummaryStartsInProposed()
    {
        var item = RetroPaySummary.NewRetroPaySummary("code-852", "label-960");
        Check.Equal(item.Status, RetroPaySummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RetroPaySummaryMovesToApproved()
    {
        var item = RetroPaySummary.NewRetroPaySummary("code-852", "label-960");
        try
        {
            item.MoveTo(RetroPaySummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RetroPaySummaryStatus.Approved, "status");
    }

    public static void RetroPaySummaryRejectsMovesFromWithdrawn()
    {
        var item = RetroPaySummary.NewRetroPaySummary("code-852", "label-960");
        item.Status = RetroPaySummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RetroPaySummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RetroPaySummaryStartsInProposed();
        RetroPaySummaryMovesToApproved();
        RetroPaySummaryRejectsMovesFromWithdrawn();
    }
}
