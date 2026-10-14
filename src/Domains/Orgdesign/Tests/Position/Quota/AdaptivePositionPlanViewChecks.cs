namespace Acme.Workforce.Orgdesign.Tests.Position.Quota;

using Acme.Workforce.Orgdesign.Position.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptivePositionPlanViewChecks
{
    public static void AdaptivePositionPlanViewShowsTheStatusInCapitals()
    {
        var item = AdaptivePositionPlan.NewAdaptivePositionPlan("code-422", "name-343");
        var view = AdaptivePositionPlanView.ToAdaptivePositionPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptivePositionPlanView.ToAdaptivePositionPlanViews(new List<AdaptivePositionPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptivePositionPlanViewShowsTheStatusInCapitals();
    }
}
