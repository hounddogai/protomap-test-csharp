namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class MeritCycleSummaryViewChecks
{
    public static void MeritCycleSummaryViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleSummary.NewMeritCycleSummary("code-839", "label-237");
        var view = MeritCycleSummaryView.ToMeritCycleSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleSummaryView.ToMeritCycleSummaryViews(new List<MeritCycleSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleSummaryViewShowsTheStatusInCapitals();
    }
}
