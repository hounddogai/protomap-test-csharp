namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class MeritCyclePolicyViewChecks
{
    public static void MeritCyclePolicyViewShowsTheStatusInCapitals()
    {
        var item = MeritCyclePolicy.NewMeritCyclePolicy("code-385", "label-130");
        var view = MeritCyclePolicyView.ToMeritCyclePolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCyclePolicyView.ToMeritCyclePolicyViews(new List<MeritCyclePolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCyclePolicyViewShowsTheStatusInCapitals();
    }
}
