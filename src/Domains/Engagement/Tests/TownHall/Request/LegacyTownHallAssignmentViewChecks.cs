namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyTownHallAssignmentViewChecks
{
    public static void LegacyTownHallAssignmentViewShowsTheStatusInCapitals()
    {
        var item = LegacyTownHallAssignment.NewLegacyTownHallAssignment("code-254", "label-490");
        var view = LegacyTownHallAssignmentView.ToLegacyTownHallAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyTownHallAssignmentView.ToLegacyTownHallAssignmentViews(new List<LegacyTownHallAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyTownHallAssignmentViewShowsTheStatusInCapitals();
    }
}
