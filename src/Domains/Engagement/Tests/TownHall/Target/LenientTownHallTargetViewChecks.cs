namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientTownHallTargetViewChecks
{
    public static void LenientTownHallTargetViewShowsTheStatusInCapitals()
    {
        var item = LenientTownHallTarget.NewLenientTownHallTarget("code-533", "name-152");
        var view = LenientTownHallTargetView.ToLenientTownHallTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientTownHallTargetView.ToLenientTownHallTargetViews(new List<LenientTownHallTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientTownHallTargetViewShowsTheStatusInCapitals();
    }
}
