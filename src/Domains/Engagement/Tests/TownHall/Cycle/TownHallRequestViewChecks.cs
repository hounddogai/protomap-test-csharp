namespace Acme.Workforce.Engagement.Tests.TownHall.Cycle;

using Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class TownHallRequestViewChecks
{
    public static void TownHallRequestViewShowsTheStatusInCapitals()
    {
        var item = TownHallRequest.NewTownHallRequest("code-219", "label-105");
        var view = TownHallRequestView.ToTownHallRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallRequestView.ToTownHallRequestViews(new List<TownHallRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallRequestViewShowsTheStatusInCapitals();
    }
}
