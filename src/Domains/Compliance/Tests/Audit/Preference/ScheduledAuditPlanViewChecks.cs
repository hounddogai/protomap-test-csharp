namespace Acme.Workforce.Compliance.Tests.Audit.Preference;

using Acme.Workforce.Compliance.Audit.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledAuditPlanViewChecks
{
    public static void ScheduledAuditPlanViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAuditPlan.NewScheduledAuditPlan("code-823", "name-11");
        var view = ScheduledAuditPlanView.ToScheduledAuditPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAuditPlanView.ToScheduledAuditPlanViews(new List<ScheduledAuditPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAuditPlanViewShowsTheStatusInCapitals();
    }
}
