namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class WeightedAccessBadgeLedgerChecks
{
    public static void WeightedAccessBadgeLedgerStartsInPlanned()
    {
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("code-141", "name-831");
        Check.Equal(item.Status, WeightedAccessBadgeLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedAccessBadgeLedgerMovesToScheduled()
    {
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("code-141", "name-831");
        try
        {
            item.MoveTo(WeightedAccessBadgeLedgerStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedAccessBadgeLedgerStatus.Scheduled, "status");
    }

    public static void WeightedAccessBadgeLedgerRejectsMovesFromCancelled()
    {
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("code-141", "name-831");
        item.Status = WeightedAccessBadgeLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(WeightedAccessBadgeLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeLedgerStartsInPlanned();
        WeightedAccessBadgeLedgerMovesToScheduled();
        WeightedAccessBadgeLedgerRejectsMovesFromCancelled();
    }
}
