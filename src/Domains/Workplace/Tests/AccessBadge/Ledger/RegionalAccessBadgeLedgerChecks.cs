namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAccessBadgeLedgerChecks
{
    public static void RegionalAccessBadgeLedgerStartsInPlanned()
    {
        var item = RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger("code-78", "label-959");
        Check.Equal(item.Status, RegionalAccessBadgeLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAccessBadgeLedgerMovesToCancelled()
    {
        var item = RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger("code-78", "label-959");
        try
        {
            item.MoveTo(RegionalAccessBadgeLedgerStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAccessBadgeLedgerStatus.Cancelled, "status");
    }

    public static void RegionalAccessBadgeLedgerRejectsMovesFromCancelled()
    {
        var item = RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger("code-78", "label-959");
        item.Status = RegionalAccessBadgeLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RegionalAccessBadgeLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RegionalAccessBadgeLedgerStartsInPlanned();
        RegionalAccessBadgeLedgerMovesToCancelled();
        RegionalAccessBadgeLedgerRejectsMovesFromCancelled();
    }
}
