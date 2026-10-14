namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class LenientMeritCycleClaimViewChecks
{
    public static void LenientMeritCycleClaimViewShowsTheStatusInCapitals()
    {
        var item = LenientMeritCycleClaim.NewLenientMeritCycleClaim("code-620", "title-334");
        var view = LenientMeritCycleClaimView.ToLenientMeritCycleClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientMeritCycleClaimView.ToLenientMeritCycleClaimViews(new List<LenientMeritCycleClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientMeritCycleClaimViewShowsTheStatusInCapitals();
    }
}
