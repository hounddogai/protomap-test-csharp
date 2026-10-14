namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolAuditChecks
{
    public static void BonusPoolAuditStartsInPlanned()
    {
        var item = BonusPoolAudit.NewBonusPoolAudit("code-651", "name-395");
        Check.Equal(item.Status, BonusPoolAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolAuditMovesToCancelled()
    {
        var item = BonusPoolAudit.NewBonusPoolAudit("code-651", "name-395");
        try
        {
            item.MoveTo(BonusPoolAuditStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolAuditStatus.Cancelled, "status");
    }

    public static void BonusPoolAuditRejectsMovesFromCancelled()
    {
        var item = BonusPoolAudit.NewBonusPoolAudit("code-651", "name-395");
        item.Status = BonusPoolAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BonusPoolAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BonusPoolAuditStartsInPlanned();
        BonusPoolAuditMovesToCancelled();
        BonusPoolAuditRejectsMovesFromCancelled();
    }
}
