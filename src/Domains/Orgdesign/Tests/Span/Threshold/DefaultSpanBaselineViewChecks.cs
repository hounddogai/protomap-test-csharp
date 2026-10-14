namespace Acme.Workforce.Orgdesign.Tests.Span.Threshold;

using Acme.Workforce.Orgdesign.Span.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class DefaultSpanBaselineViewChecks
{
    public static void DefaultSpanBaselineViewShowsTheStatusInCapitals()
    {
        var item = DefaultSpanBaseline.NewDefaultSpanBaseline("code-946", "name-22");
        var view = DefaultSpanBaselineView.ToDefaultSpanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultSpanBaselineView.ToDefaultSpanBaselineViews(new List<DefaultSpanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultSpanBaselineViewShowsTheStatusInCapitals();
    }
}
