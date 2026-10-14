namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class GlobalPayBandBaselineViewChecks
{
    public static void GlobalPayBandBaselineViewShowsTheStatusInCapitals()
    {
        var item = GlobalPayBandBaseline.NewGlobalPayBandBaseline("code-754", "name-319");
        var view = GlobalPayBandBaselineView.ToGlobalPayBandBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPayBandBaselineView.ToGlobalPayBandBaselineViews(new List<GlobalPayBandBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPayBandBaselineViewShowsTheStatusInCapitals();
    }
}
