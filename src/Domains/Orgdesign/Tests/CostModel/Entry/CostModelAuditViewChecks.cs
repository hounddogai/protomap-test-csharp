namespace Acme.Workforce.Orgdesign.Tests.CostModel.Entry;

using Acme.Workforce.Orgdesign.CostModel.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CostModelAuditViewChecks
{
    public static void CostModelAuditViewShowsTheStatusInCapitals()
    {
        var item = CostModelAudit.NewCostModelAudit("code-297", "title-581");
        var view = CostModelAuditView.ToCostModelAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CostModelAuditView.ToCostModelAuditViews(new List<CostModelAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CostModelAuditViewShowsTheStatusInCapitals();
    }
}
