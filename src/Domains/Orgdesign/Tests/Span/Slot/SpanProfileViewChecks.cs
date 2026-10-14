namespace Acme.Workforce.Orgdesign.Tests.Span.Slot;

using Acme.Workforce.Orgdesign.Span.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class SpanProfileViewChecks
{
    public static void SpanProfileViewShowsTheStatusInCapitals()
    {
        var item = SpanProfile.NewSpanProfile("code-753", "label-297");
        var view = SpanProfileView.ToSpanProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanProfileView.ToSpanProfileViews(new List<SpanProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanProfileViewShowsTheStatusInCapitals();
    }
}
