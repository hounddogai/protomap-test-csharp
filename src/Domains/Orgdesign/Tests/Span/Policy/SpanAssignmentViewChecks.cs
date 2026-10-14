namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class SpanAssignmentViewChecks
{
    public static void SpanAssignmentViewShowsTheStatusInCapitals()
    {
        var item = SpanAssignment.NewSpanAssignment("code-320", "name-701");
        var view = SpanAssignmentView.ToSpanAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SpanAssignmentView.ToSpanAssignmentViews(new List<SpanAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SpanAssignmentViewShowsTheStatusInCapitals();
    }
}
