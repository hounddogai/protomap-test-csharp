namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SpanRuleViewChecks
{
    public static void SpanRuleViewShowsTheStatusInCapitals()
    {
        var item = SpanRule.NewSpanRule("code-203", "title-899");
        var view = SpanRuleView.ToSpanRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanRuleView.ToSpanRuleViews(new List<SpanRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanRuleViewShowsTheStatusInCapitals();
    }
}
