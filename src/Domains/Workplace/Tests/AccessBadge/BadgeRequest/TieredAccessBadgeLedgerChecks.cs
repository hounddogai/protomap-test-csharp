namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeRequest;

using Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

/// <summary>Checks of badge request code, run by the project's runner.</summary>
public static class TieredAccessBadgeLedgerChecks
{
    public static void TieredAccessBadgeLedgerStartsInPlanned()
    {
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("code-282", "label-376");
        Check.Equal(item.Status, TieredAccessBadgeLedgerStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredAccessBadgeLedgerMovesToInProgress()
    {
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("code-282", "label-376");
        try
        {
            item.MoveTo(TieredAccessBadgeLedgerStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredAccessBadgeLedgerStatus.InProgress, "status");
    }

    public static void TieredAccessBadgeLedgerRejectsMovesFromCancelled()
    {
        var item = TieredAccessBadgeLedger.NewTieredAccessBadgeLedger("code-282", "label-376");
        item.Status = TieredAccessBadgeLedgerStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TieredAccessBadgeLedgerStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TieredAccessBadgeLedgerStartsInPlanned();
        TieredAccessBadgeLedgerMovesToInProgress();
        TieredAccessBadgeLedgerRejectsMovesFromCancelled();
    }
}
