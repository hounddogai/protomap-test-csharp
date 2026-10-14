namespace Acme.Workforce.Compensation.Tests.PayBand.BandBaseline;

using Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>Checks of band baseline code, run by the project's runner.</summary>
public static class ScheduledPayBandManifestViewChecks
{
    public static void ScheduledPayBandManifestViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayBandManifest.NewScheduledPayBandManifest("code-387", "title-439");
        var view = ScheduledPayBandManifestView.ToScheduledPayBandManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayBandManifestView.ToScheduledPayBandManifestViews(new List<ScheduledPayBandManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayBandManifestViewShowsTheStatusInCapitals();
    }
}
