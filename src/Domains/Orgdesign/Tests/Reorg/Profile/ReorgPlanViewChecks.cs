namespace Acme.Workforce.Orgdesign.Tests.Reorg.Profile;

using Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ReorgPlanViewChecks
{
    public static void ReorgPlanViewShowsTheStatusInCapitals()
    {
        var item = ReorgPlan.NewReorgPlan("code-321", "label-454");
        var view = ReorgPlanView.ToReorgPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ReorgPlanView.ToReorgPlanViews(new List<ReorgPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ReorgPlanViewShowsTheStatusInCapitals();
    }
}
