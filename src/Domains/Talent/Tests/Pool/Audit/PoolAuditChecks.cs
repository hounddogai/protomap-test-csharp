namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolAuditChecks
{
    public static void PoolAuditStartsInDraft()
    {
        var item = PoolAudit.NewPoolAudit("code-510", "title-142");
        Check.Equal(item.Status, PoolAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolAuditMovesToPending()
    {
        var item = PoolAudit.NewPoolAudit("code-510", "title-142");
        try
        {
            item.MoveTo(PoolAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolAuditStatus.Pending, "status");
    }

    public static void PoolAuditRejectsMovesFromClosed()
    {
        var item = PoolAudit.NewPoolAudit("code-510", "title-142");
        item.Status = PoolAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(PoolAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PoolAuditStartsInDraft();
        PoolAuditMovesToPending();
        PoolAuditRejectsMovesFromClosed();
    }
}
