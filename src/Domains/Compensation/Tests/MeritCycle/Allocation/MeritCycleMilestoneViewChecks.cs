namespace Acme.Workforce.Compensation.Tests.MeritCycle.Allocation;

using Acme.Workforce.Compensation.MeritCycle.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class MeritCycleMilestoneViewChecks
{
    public static void MeritCycleMilestoneViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleMilestone.NewMeritCycleMilestone("code-178", "label-369");
        var view = MeritCycleMilestoneView.ToMeritCycleMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleMilestoneView.ToMeritCycleMilestoneViews(new List<MeritCycleMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleMilestoneViewShowsTheStatusInCapitals();
    }
}
