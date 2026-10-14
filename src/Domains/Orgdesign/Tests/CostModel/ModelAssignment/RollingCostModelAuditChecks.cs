namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RollingCostModelAuditChecks
{
    public static void RollingCostModelAuditStartsInNew()
    {
        var item = RollingCostModelAudit.NewRollingCostModelAudit("code-502", "name-511");
        Check.Equal(item.Status, RollingCostModelAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingCostModelAuditMovesToQueued()
    {
        var item = RollingCostModelAudit.NewRollingCostModelAudit("code-502", "name-511");
        try
        {
            item.MoveTo(RollingCostModelAuditStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingCostModelAuditStatus.Queued, "status");
    }

    public static void RollingCostModelAuditRejectsMovesFromFailed()
    {
        var item = RollingCostModelAudit.NewRollingCostModelAudit("code-502", "name-511");
        item.Status = RollingCostModelAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingCostModelAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingCostModelAuditStartsInNew();
        RollingCostModelAuditMovesToQueued();
        RollingCostModelAuditRejectsMovesFromFailed();
    }
}
