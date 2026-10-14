namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PrimaryMeritCycleWindowViewChecks
{
    public static void PrimaryMeritCycleWindowViewShowsTheStatusInCapitals()
    {
        var item = PrimaryMeritCycleWindow.NewPrimaryMeritCycleWindow("code-880", "name-186");
        var view = PrimaryMeritCycleWindowView.ToPrimaryMeritCycleWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryMeritCycleWindowView.ToPrimaryMeritCycleWindowViews(new List<PrimaryMeritCycleWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleWindowViewShowsTheStatusInCapitals();
    }
}
