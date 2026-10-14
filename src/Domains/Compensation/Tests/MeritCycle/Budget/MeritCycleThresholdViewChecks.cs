namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleThresholdViewChecks
{
    public static void MeritCycleThresholdViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleThreshold.NewMeritCycleThreshold("code-261", "title-501");
        var view = MeritCycleThresholdView.ToMeritCycleThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleThresholdView.ToMeritCycleThresholdViews(new List<MeritCycleThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleThresholdViewShowsTheStatusInCapitals();
    }
}
