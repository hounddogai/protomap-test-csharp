namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LenientMeritCycleWindowViewChecks
{
    public static void LenientMeritCycleWindowViewShowsTheStatusInCapitals()
    {
        var item = LenientMeritCycleWindow.NewLenientMeritCycleWindow("code-276", "name-554");
        var view = LenientMeritCycleWindowView.ToLenientMeritCycleWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientMeritCycleWindowView.ToLenientMeritCycleWindowViews(new List<LenientMeritCycleWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientMeritCycleWindowViewShowsTheStatusInCapitals();
    }
}
