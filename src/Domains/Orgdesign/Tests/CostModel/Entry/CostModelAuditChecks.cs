namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelAuditChecks
{
    public static void CostModelAuditStartsInRequested()
    {
        var item = CostModelAudit.NewCostModelAudit("code-297", "title-581");
        Check.Equal(item.Status, CostModelAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CostModelAuditMovesToRevoked()
    {
        var item = CostModelAudit.NewCostModelAudit("code-297", "title-581");
        try
        {
            item.MoveTo(CostModelAuditStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CostModelAuditStatus.Revoked, "status");
    }

    public static void CostModelAuditRejectsMovesFromExpired()
    {
        var item = CostModelAudit.NewCostModelAudit("code-297", "title-581");
        item.Status = CostModelAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(CostModelAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CostModelAuditStartsInRequested();
        CostModelAuditMovesToRevoked();
        CostModelAuditRejectsMovesFromExpired();
    }
}
