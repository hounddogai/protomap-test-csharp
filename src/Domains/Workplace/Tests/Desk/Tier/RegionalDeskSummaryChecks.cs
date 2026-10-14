namespace Acme.Workforce.Workplace.Tests.Desk.Tier;

using Acme.Workforce.Workplace.Desk.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RegionalDeskSummaryChecks
{
    public static void RegionalDeskSummaryStartsInProposed()
    {
        var item = RegionalDeskSummary.NewRegionalDeskSummary("code-286", "title-923");
        Check.Equal(item.Status, RegionalDeskSummaryStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalDeskSummaryMovesToApproved()
    {
        var item = RegionalDeskSummary.NewRegionalDeskSummary("code-286", "title-923");
        try
        {
            item.MoveTo(RegionalDeskSummaryStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalDeskSummaryStatus.Approved, "status");
    }

    public static void RegionalDeskSummaryRejectsMovesFromWithdrawn()
    {
        var item = RegionalDeskSummary.NewRegionalDeskSummary("code-286", "title-923");
        item.Status = RegionalDeskSummaryStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalDeskSummaryStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalDeskSummaryStartsInProposed();
        RegionalDeskSummaryMovesToApproved();
        RegionalDeskSummaryRejectsMovesFromWithdrawn();
    }
}
