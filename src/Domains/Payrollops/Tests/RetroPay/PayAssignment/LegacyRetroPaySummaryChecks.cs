namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class LegacyRetroPaySummaryChecks
{
    public static void LegacyRetroPaySummaryStartsInProposed()
    {
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("code-865", "title-718");
        Check.Equal(item.Status, LegacyRetroPaySummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRetroPaySummaryMovesToRejected()
    {
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("code-865", "title-718");
        try
        {
            item.MoveTo(LegacyRetroPaySummaryStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRetroPaySummaryStatus.Rejected, "status");
    }

    public static void LegacyRetroPaySummaryRejectsMovesFromWithdrawn()
    {
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("code-865", "title-718");
        item.Status = LegacyRetroPaySummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyRetroPaySummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyRetroPaySummaryStartsInProposed();
        LegacyRetroPaySummaryMovesToRejected();
        LegacyRetroPaySummaryRejectsMovesFromWithdrawn();
    }
}
