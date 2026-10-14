namespace Acme.Workforce.Orgdesign.Tests.Span.TieredSpanDraft;

using Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

/// <summary>Checks of tiered span draft code, run by the project's runner.</summary>
public static class SpanManifestViewChecks
{
    public static void SpanManifestViewShowsTheStatusInCapitals()
    {
        var item = SpanManifest.NewSpanManifest("code-151", "title-379");
        var view = SpanManifestView.ToSpanManifestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanManifestView.ToSpanManifestViews(new List<SpanManifest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanManifestViewShowsTheStatusInCapitals();
    }
}
