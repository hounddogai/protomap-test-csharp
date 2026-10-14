namespace Acme.Workforce.Orgdesign.Tests.Span.Template;

using Acme.Workforce.Orgdesign.Span.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class SpanBudgetViewChecks
{
    public static void SpanBudgetViewShowsTheStatusInCapitals()
    {
        var item = SpanBudget.NewSpanBudget("code-591", "name-370");
        var view = SpanBudgetView.ToSpanBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanBudgetView.ToSpanBudgetViews(new List<SpanBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanBudgetViewShowsTheStatusInCapitals();
    }
}
