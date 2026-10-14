namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleOverrideViewChecks
{
    public static void MeritCycleOverrideViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleOverride.NewMeritCycleOverride("code-517", "label-833");
        var view = MeritCycleOverrideView.ToMeritCycleOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleOverrideView.ToMeritCycleOverrideViews(new List<MeritCycleOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleOverrideViewShowsTheStatusInCapitals();
    }
}
