namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyPositionPlanViewChecks
{
    public static void LegacyPositionPlanViewShowsTheStatusInCapitals()
    {
        var item = LegacyPositionPlan.NewLegacyPositionPlan("code-610", "name-157");
        var view = LegacyPositionPlanView.ToLegacyPositionPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPositionPlanView.ToLegacyPositionPlanViews(new List<LegacyPositionPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPositionPlanViewShowsTheStatusInCapitals();
    }
}
