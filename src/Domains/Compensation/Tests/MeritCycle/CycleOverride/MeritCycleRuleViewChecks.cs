namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleRuleViewChecks
{
    public static void MeritCycleRuleViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleRule.NewMeritCycleRule("code-410", "title-395");
        var view = MeritCycleRuleView.ToMeritCycleRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleRuleView.ToMeritCycleRuleViews(new List<MeritCycleRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleRuleViewShowsTheStatusInCapitals();
    }
}
