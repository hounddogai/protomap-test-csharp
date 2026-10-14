namespace Acme.Workforce.Compensation.Tests.MeritCycle.Review;

using Acme.Workforce.Compensation.MeritCycle.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class MeritCycleAuditViewChecks
{
    public static void MeritCycleAuditViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleAudit.NewMeritCycleAudit("code-281", "name-494");
        var view = MeritCycleAuditView.ToMeritCycleAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleAuditView.ToMeritCycleAuditViews(new List<MeritCycleAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleAuditViewShowsTheStatusInCapitals();
    }
}
