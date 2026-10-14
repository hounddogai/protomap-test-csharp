namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantDigestViewChecks
{
    public static void EquityGrantDigestViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantDigest.NewEquityGrantDigest("code-785", "label-856");
        var view = EquityGrantDigestView.ToEquityGrantDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantDigestView.ToEquityGrantDigestViews(new List<EquityGrantDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantDigestViewShowsTheStatusInCapitals();
    }
}
