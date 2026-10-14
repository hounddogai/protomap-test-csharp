namespace Acme.Workforce.Compensation.Tests.EquityGrant.Agreement;

using Acme.Workforce.Compensation.EquityGrant.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PrimaryEquityGrantSummaryChecks
{
    public static void PrimaryEquityGrantSummaryStartsInDraft()
    {
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("code-262", "title-548");
        Check.Equal(item.Status, PrimaryEquityGrantSummaryStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryEquityGrantSummaryMovesToActive()
    {
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("code-262", "title-548");
        try
        {
            item.MoveTo(PrimaryEquityGrantSummaryStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryEquityGrantSummaryStatus.Active, "status");
    }

    public static void PrimaryEquityGrantSummaryRejectsMovesFromClosed()
    {
        var item = PrimaryEquityGrantSummary.NewPrimaryEquityGrantSummary("code-262", "title-548");
        item.Status = PrimaryEquityGrantSummaryStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryEquityGrantSummaryStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryEquityGrantSummaryStartsInDraft();
        PrimaryEquityGrantSummaryMovesToActive();
        PrimaryEquityGrantSummaryRejectsMovesFromClosed();
    }
}
