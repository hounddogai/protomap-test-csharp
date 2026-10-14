namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleTargetViewChecks
{
    public static void MeritCycleTargetViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleTarget.NewMeritCycleTarget("code-101", "name-67");
        var view = MeritCycleTargetView.ToMeritCycleTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleTargetView.ToMeritCycleTargetViews(new List<MeritCycleTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleTargetViewShowsTheStatusInCapitals();
    }
}
