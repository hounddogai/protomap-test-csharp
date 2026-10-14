namespace Acme.Workforce.Workplace.Tests.AccessBadge.Entry;

using Acme.Workforce.Workplace.AccessBadge.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class AccessBadgeLedgerChecks
{
    public static void AccessBadgeLedgerStartsInNew()
    {
        var item = AccessBadgeLedger.NewAccessBadgeLedger("code-558", "label-682");
        Check.Equal(item.Status, AccessBadgeLedgerStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeLedgerMovesToFailed()
    {
        var item = AccessBadgeLedger.NewAccessBadgeLedger("code-558", "label-682");
        try
        {
            item.MoveTo(AccessBadgeLedgerStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeLedgerStatus.Failed, "status");
    }

    public static void AccessBadgeLedgerRejectsMovesFromFailed()
    {
        var item = AccessBadgeLedger.NewAccessBadgeLedger("code-558", "label-682");
        item.Status = AccessBadgeLedgerStatus.Failed;
        Check.Fails(() => item.MoveTo(AccessBadgeLedgerStatus.New), "move");
    }

    public static void RunAll()
    {
        AccessBadgeLedgerStartsInNew();
        AccessBadgeLedgerMovesToFailed();
        AccessBadgeLedgerRejectsMovesFromFailed();
    }
}
