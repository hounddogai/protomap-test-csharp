namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentDigestViewChecks
{
    public static void GarnishmentDigestViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentDigest.NewGarnishmentDigest("code-833", "label-310");
        var view = GarnishmentDigestView.ToGarnishmentDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentDigestView.ToGarnishmentDigestViews(new List<GarnishmentDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentDigestViewShowsTheStatusInCapitals();
    }
}
