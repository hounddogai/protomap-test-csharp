namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolDigestViewChecks
{
    public static void PoolDigestViewShowsTheStatusInCapitals()
    {
        var item = PoolDigest.NewPoolDigest("code-920", "title-444");
        var view = PoolDigestView.ToPoolDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PoolDigestView.ToPoolDigestViews(new List<PoolDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PoolDigestViewShowsTheStatusInCapitals();
    }
}
