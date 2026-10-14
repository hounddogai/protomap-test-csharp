namespace Acme.Workforce.Engagement.Tests.TownHall.Tier;

using Acme.Workforce.Engagement.TownHall.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TownHallPlanViewChecks
{
    public static void TownHallPlanViewShowsTheStatusInCapitals()
    {
        var item = TownHallPlan.NewTownHallPlan("code-498", "name-924");
        var view = TownHallPlanView.ToTownHallPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallPlanView.ToTownHallPlanViews(new List<TownHallPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallPlanViewShowsTheStatusInCapitals();
    }
}
