namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateAuditViewChecks
{
    public static void RateAuditViewShowsTheStatusInCapitals()
    {
        var item = RateAudit.NewRateAudit("code-929", "name-968");
        var view = RateAuditView.ToRateAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateAuditView.ToRateAuditViews(new List<RateAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateAuditViewShowsTheStatusInCapitals();
    }
}
