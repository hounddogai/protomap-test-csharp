namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CappedPoolAuditViewChecks
{
    public static void CappedPoolAuditViewShowsTheStatusInCapitals()
    {
        var item = CappedPoolAudit.NewCappedPoolAudit("code-575", "label-672");
        var view = CappedPoolAuditView.ToCappedPoolAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedPoolAuditView.ToCappedPoolAuditViews(new List<CappedPoolAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedPoolAuditViewShowsTheStatusInCapitals();
    }
}
