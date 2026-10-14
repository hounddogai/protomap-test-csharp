namespace Acme.Workforce.Contractors.Tests.Rate.Tier;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RateLedgerChecks
{
    public static void RateLedgerStartsInOpen()
    {
        var item = RateLedger.NewRateLedger("code-701", "name-229");
        Check.Equal(item.Status, RateLedgerStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateLedgerMovesToResolved()
    {
        var item = RateLedger.NewRateLedger("code-701", "name-229");
        try
        {
            item.MoveTo(RateLedgerStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateLedgerStatus.Resolved, "status");
    }

    public static void RateLedgerRejectsMovesFromArchived()
    {
        var item = RateLedger.NewRateLedger("code-701", "name-229");
        item.Status = RateLedgerStatus.Archived;
        Check.Fails(() => item.MoveTo(RateLedgerStatus.Open), "move");
    }

    public static void RunAll()
    {
        RateLedgerStartsInOpen();
        RateLedgerMovesToResolved();
        RateLedgerRejectsMovesFromArchived();
    }
}
