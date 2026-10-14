namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleTarget;

using Acme.Workforce.Compensation.MeritCycle.CycleTarget;

/// <summary>Checks of cycle target code, run by the project's runner.</summary>
public static class MeritCycleDigestViewChecks
{
    public static void MeritCycleDigestViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleDigest.NewMeritCycleDigest("code-321", "name-858");
        var view = MeritCycleDigestView.ToMeritCycleDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleDigestView.ToMeritCycleDigestViews(new List<MeritCycleDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleDigestViewShowsTheStatusInCapitals();
    }
}
