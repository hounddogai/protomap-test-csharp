namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CappedMeritCycleDigestViewChecks
{
    public static void CappedMeritCycleDigestViewShowsTheStatusInCapitals()
    {
        var item = CappedMeritCycleDigest.NewCappedMeritCycleDigest("code-293", "title-702");
        var view = CappedMeritCycleDigestView.ToCappedMeritCycleDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedMeritCycleDigestView.ToCappedMeritCycleDigestViews(new List<CappedMeritCycleDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedMeritCycleDigestViewShowsTheStatusInCapitals();
    }
}
