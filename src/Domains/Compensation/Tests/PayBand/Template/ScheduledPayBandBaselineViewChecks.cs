namespace Acme.Workforce.Compensation.Tests.PayBand.Template;

using Acme.Workforce.Compensation.PayBand.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ScheduledPayBandBaselineViewChecks
{
    public static void ScheduledPayBandBaselineViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayBandBaseline.NewScheduledPayBandBaseline("code-669", "title-544");
        var view = ScheduledPayBandBaselineView.ToScheduledPayBandBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayBandBaselineView.ToScheduledPayBandBaselineViews(new List<ScheduledPayBandBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayBandBaselineViewShowsTheStatusInCapitals();
    }
}
