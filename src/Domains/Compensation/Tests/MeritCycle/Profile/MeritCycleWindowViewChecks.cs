namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleWindowViewChecks
{
    public static void MeritCycleWindowViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleWindow.NewMeritCycleWindow("code-538", "name-236");
        var view = MeritCycleWindowView.ToMeritCycleWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleWindowView.ToMeritCycleWindowViews(new List<MeritCycleWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleWindowViewShowsTheStatusInCapitals();
    }
}
