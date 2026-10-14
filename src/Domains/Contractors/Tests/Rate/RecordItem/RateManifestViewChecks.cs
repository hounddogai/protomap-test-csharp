namespace Acme.Workforce.Contractors.Tests.Rate.RecordItem;

using Acme.Workforce.Contractors.Rate.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RateManifestViewChecks
{
    public static void RateManifestViewShowsTheStatusInCapitals()
    {
        var item = RateManifest.NewRateManifest("code-863", "label-817");
        var view = RateManifestView.ToRateManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateManifestView.ToRateManifestViews(new List<RateManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateManifestViewShowsTheStatusInCapitals();
    }
}
