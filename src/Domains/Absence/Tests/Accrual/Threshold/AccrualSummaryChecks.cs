namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualSummaryChecks
{
    public static void AccrualSummaryStartsInDraft()
    {
        var item = AccrualSummary.NewAccrualSummary("code-816", "name-874");
        Check.Equal(item.Status, AccrualSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualSummaryMovesToActive()
    {
        var item = AccrualSummary.NewAccrualSummary("code-816", "name-874");
        try
        {
            item.MoveTo(AccrualSummaryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualSummaryStatus.Active, "status");
    }

    public static void AccrualSummaryRejectsMovesFromClosed()
    {
        var item = AccrualSummary.NewAccrualSummary("code-816", "name-874");
        item.Status = AccrualSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(AccrualSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccrualSummaryStartsInDraft();
        AccrualSummaryMovesToActive();
        AccrualSummaryRejectsMovesFromClosed();
    }
}
