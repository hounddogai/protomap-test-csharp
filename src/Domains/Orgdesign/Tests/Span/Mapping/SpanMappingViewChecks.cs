namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class SpanMappingViewChecks
{
    public static void SpanMappingViewShowsTheStatusInCapitals()
    {
        var item = SpanMapping.NewSpanMapping("code-68", "label-44");
        var view = SpanMappingView.ToSpanMappingView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanMappingView.ToSpanMappingViews(new List<SpanMapping> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanMappingViewShowsTheStatusInCapitals();
    }
}
