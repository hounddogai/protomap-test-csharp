namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CappedPoolAuditChecks
{
    public static void CappedPoolAuditStartsInRequested()
    {
        var item = CappedPoolAudit.NewCappedPoolAudit("code-575", "label-672");
        Check.Equal(item.Status, CappedPoolAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPoolAuditMovesToRevoked()
    {
        var item = CappedPoolAudit.NewCappedPoolAudit("code-575", "label-672");
        try
        {
            item.MoveTo(CappedPoolAuditStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPoolAuditStatus.Revoked, "status");
    }

    public static void CappedPoolAuditRejectsMovesFromExpired()
    {
        var item = CappedPoolAudit.NewCappedPoolAudit("code-575", "label-672");
        item.Status = CappedPoolAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedPoolAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedPoolAuditStartsInRequested();
        CappedPoolAuditMovesToRevoked();
        CappedPoolAuditRejectsMovesFromExpired();
    }
}
