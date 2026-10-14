namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class MeritCycleGroupViewChecks
{
    public static void MeritCycleGroupViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleGroup.NewMeritCycleGroup("code-329", "name-936");
        var view = MeritCycleGroupView.ToMeritCycleGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleGroupView.ToMeritCycleGroupViews(new List<MeritCycleGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleGroupViewShowsTheStatusInCapitals();
    }
}
