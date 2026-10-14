namespace Acme.Workforce.Orgdesign.Tests.Span.Draft;

using Acme.Workforce.Orgdesign.Span.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class ScheduledSpanDigestViewChecks
{
    public static void ScheduledSpanDigestViewShowsTheStatusInCapitals()
    {
        var item = ScheduledSpanDigest.NewScheduledSpanDigest("code-664", "label-658");
        var view = ScheduledSpanDigestView.ToScheduledSpanDigestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledSpanDigestView.ToScheduledSpanDigestViews(new List<ScheduledSpanDigest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledSpanDigestViewShowsTheStatusInCapitals();
    }
}
