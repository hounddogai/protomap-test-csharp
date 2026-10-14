namespace Acme.Workforce.Compliance.Tests.Exception.Plan;

using Acme.Workforce.Compliance.Exception.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingExceptionPlanViewChecks
{
    public static void PendingExceptionPlanViewShowsTheStatusInCapitals()
    {
        var item = PendingExceptionPlan.NewPendingExceptionPlan("code-812", "title-271");
        var view = PendingExceptionPlanView.ToPendingExceptionPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingExceptionPlanView.ToPendingExceptionPlanViews(new List<PendingExceptionPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingExceptionPlanViewShowsTheStatusInCapitals();
    }
}
