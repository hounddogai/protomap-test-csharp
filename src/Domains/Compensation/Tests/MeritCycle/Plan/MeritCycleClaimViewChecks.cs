namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleClaimViewChecks
{
    public static void MeritCycleClaimViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleClaim.NewMeritCycleClaim("code-211", "title-330");
        var view = MeritCycleClaimView.ToMeritCycleClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleClaimView.ToMeritCycleClaimViews(new List<MeritCycleClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleClaimViewShowsTheStatusInCapitals();
    }
}
