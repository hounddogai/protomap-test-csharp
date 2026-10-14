namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RegionalAccrualSummaryChecks
{
    public static void RegionalAccrualSummaryStartsInOpen()
    {
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("code-725", "title-463");
        Check.Equal(item.Status, RegionalAccrualSummaryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAccrualSummaryMovesToResolved()
    {
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("code-725", "title-463");
        try
        {
            item.MoveTo(RegionalAccrualSummaryStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAccrualSummaryStatus.Resolved, "status");
    }

    public static void RegionalAccrualSummaryRejectsMovesFromArchived()
    {
        var item = RegionalAccrualSummary.NewRegionalAccrualSummary("code-725", "title-463");
        item.Status = RegionalAccrualSummaryStatus.Archived;
        Check.Fails(() => item.MoveTo(RegionalAccrualSummaryStatus.Open), "move");
    }

    public static void RunAll()
    {
        RegionalAccrualSummaryStartsInOpen();
        RegionalAccrualSummaryMovesToResolved();
        RegionalAccrualSummaryRejectsMovesFromArchived();
    }
}
