namespace Acme.Workforce.Orgdesign.Tests.Span.Reservation;

using Acme.Workforce.Orgdesign.Span.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedSpanDigestViewChecks
{
    public static void CappedSpanDigestViewShowsTheStatusInCapitals()
    {
        var item = CappedSpanDigest.NewCappedSpanDigest("code-649", "title-4");
        var view = CappedSpanDigestView.ToCappedSpanDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedSpanDigestView.ToCappedSpanDigestViews(new List<CappedSpanDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedSpanDigestViewShowsTheStatusInCapitals();
    }
}
