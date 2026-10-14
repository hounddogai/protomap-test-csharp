namespace Acme.Workforce.Compensation.Tests.EquityGrant.Summary;

using Acme.Workforce.Compensation.EquityGrant.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class EquityGrantSummaryChecks
{
    public static void EquityGrantSummaryStartsInCreated()
    {
        var item = EquityGrantSummary.NewEquityGrantSummary("code-766", "label-159");
        Check.Equal(item.Status, EquityGrantSummaryStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void EquityGrantSummaryMovesToValidated()
    {
        var item = EquityGrantSummary.NewEquityGrantSummary("code-766", "label-159");
        try
        {
            item.MoveTo(EquityGrantSummaryStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, EquityGrantSummaryStatus.Validated, "status");
    }

    public static void EquityGrantSummaryRejectsMovesFromRetired()
    {
        var item = EquityGrantSummary.NewEquityGrantSummary("code-766", "label-159");
        item.Status = EquityGrantSummaryStatus.Retired;
        Check.Fails(() => item.MoveTo(EquityGrantSummaryStatus.Created), "move");
    }

    public static void RunAll()
    {
        EquityGrantSummaryStartsInCreated();
        EquityGrantSummaryMovesToValidated();
        EquityGrantSummaryRejectsMovesFromRetired();
    }
}
