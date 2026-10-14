namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelAssignment;

using Acme.Workforce.Orgdesign.CostModel.ModelAssignment;

/// <summary>Checks of model assignment code, run by the project's runner.</summary>
public static class RollingCostModelAuditViewChecks
{
    public static void RollingCostModelAuditViewShowsTheStatusInCapitals()
    {
        var item = RollingCostModelAudit.NewRollingCostModelAudit("code-502", "name-511");
        var view = RollingCostModelAuditView.ToRollingCostModelAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingCostModelAuditView.ToRollingCostModelAuditViews(new List<RollingCostModelAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingCostModelAuditViewShowsTheStatusInCapitals();
    }
}
