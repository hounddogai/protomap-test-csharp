namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Claim;

using Acme.Workforce.Talent.SuccessionPlan.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class SuccessionPlanQuotaViewChecks
{
    public static void SuccessionPlanQuotaViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanQuota.NewSuccessionPlanQuota("code-924", "title-683");
        var view = SuccessionPlanQuotaView.ToSuccessionPlanQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanQuotaView.ToSuccessionPlanQuotaViews(new List<SuccessionPlanQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanQuotaViewShowsTheStatusInCapitals();
    }
}
