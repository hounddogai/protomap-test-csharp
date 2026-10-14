namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PositionDigestViewChecks
{
    public static void PositionDigestViewShowsTheStatusInCapitals()
    {
        var item = PositionDigest.NewPositionDigest("code-587", "name-354");
        var view = PositionDigestView.ToPositionDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PositionDigestView.ToPositionDigestViews(new List<PositionDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PositionDigestViewShowsTheStatusInCapitals();
    }
}
