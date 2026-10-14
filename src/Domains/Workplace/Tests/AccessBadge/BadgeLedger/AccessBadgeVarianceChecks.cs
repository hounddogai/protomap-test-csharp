namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class AccessBadgeVarianceChecks
{
    public static void AccessBadgeVarianceStartsInDraft()
    {
        var item = AccessBadgeVariance.NewAccessBadgeVariance("code-595", "name-203");
        Check.Equal(item.Status, AccessBadgeVarianceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccessBadgeVarianceMovesToActive()
    {
        var item = AccessBadgeVariance.NewAccessBadgeVariance("code-595", "name-203");
        try
        {
            item.MoveTo(AccessBadgeVarianceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccessBadgeVarianceStatus.Active, "status");
    }

    public static void AccessBadgeVarianceRejectsMovesFromClosed()
    {
        var item = AccessBadgeVariance.NewAccessBadgeVariance("code-595", "name-203");
        item.Status = AccessBadgeVarianceStatus.Closed;
        Check.Fails(() => item.MoveTo(AccessBadgeVarianceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AccessBadgeVarianceStartsInDraft();
        AccessBadgeVarianceMovesToActive();
        AccessBadgeVarianceRejectsMovesFromClosed();
    }
}
