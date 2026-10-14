namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class TieredMeritCyclePlanViewChecks
{
    public static void TieredMeritCyclePlanViewShowsTheStatusInCapitals()
    {
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("code-367", "name-402");
        var view = TieredMeritCyclePlanView.ToTieredMeritCyclePlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredMeritCyclePlanView.ToTieredMeritCyclePlanViews(new List<TieredMeritCyclePlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredMeritCyclePlanViewShowsTheStatusInCapitals();
    }
}
