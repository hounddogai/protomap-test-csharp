namespace Acme.Workforce.Payrollops.Tests.PayRun.Bundle;

using Acme.Workforce.Payrollops.PayRun.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PayRunSummaryChecks
{
    public static void PayRunSummaryStartsInCreated()
    {
        var item = PayRunSummary.NewPayRunSummary("code-656", "title-91");
        Check.Equal(item.Status, PayRunSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PayRunSummaryMovesToRetired()
    {
        var item = PayRunSummary.NewPayRunSummary("code-656", "title-91");
        try
        {
            item.MoveTo(PayRunSummaryStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PayRunSummaryStatus.Retired, "status");
    }

    public static void PayRunSummaryRejectsMovesFromRetired()
    {
        var item = PayRunSummary.NewPayRunSummary("code-656", "title-91");
        item.Status = PayRunSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(PayRunSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        PayRunSummaryStartsInCreated();
        PayRunSummaryMovesToRetired();
        PayRunSummaryRejectsMovesFromRetired();
    }
}
