namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ReorgAuditViewChecks
{
    public static void ReorgAuditViewShowsTheStatusInCapitals()
    {
        var item = ReorgAudit.NewReorgAudit("code-806", "name-499");
        var view = ReorgAuditView.ToReorgAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgAuditView.ToReorgAuditViews(new List<ReorgAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgAuditViewShowsTheStatusInCapitals();
    }
}
