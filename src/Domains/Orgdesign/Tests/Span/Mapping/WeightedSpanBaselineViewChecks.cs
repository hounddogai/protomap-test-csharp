namespace Acme.Workforce.Orgdesign.Tests.Span.Mapping;

using Acme.Workforce.Orgdesign.Span.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class WeightedSpanBaselineViewChecks
{
    public static void WeightedSpanBaselineViewShowsTheStatusInCapitals()
    {
        var item = WeightedSpanBaseline.NewWeightedSpanBaseline("code-958", "title-987");
        var view = WeightedSpanBaselineView.ToWeightedSpanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedSpanBaselineView.ToWeightedSpanBaselineViews(new List<WeightedSpanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedSpanBaselineViewShowsTheStatusInCapitals();
    }
}
