namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class TownHallAssignmentViewChecks
{
    public static void TownHallAssignmentViewShowsTheStatusInCapitals()
    {
        var item = TownHallAssignment.NewTownHallAssignment("code-326", "title-163");
        var view = TownHallAssignmentView.ToTownHallAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TownHallAssignmentView.ToTownHallAssignmentViews(new List<TownHallAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TownHallAssignmentViewShowsTheStatusInCapitals();
    }
}
