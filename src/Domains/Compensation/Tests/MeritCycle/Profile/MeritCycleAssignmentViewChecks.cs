namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleAssignmentViewChecks
{
    public static void MeritCycleAssignmentViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleAssignment.NewMeritCycleAssignment("code-508", "name-329");
        var view = MeritCycleAssignmentView.ToMeritCycleAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleAssignmentView.ToMeritCycleAssignmentViews(new List<MeritCycleAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleAssignmentViewShowsTheStatusInCapitals();
    }
}
