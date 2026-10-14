namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingSpanBaselineViewChecks
{
    public static void PendingSpanBaselineViewShowsTheStatusInCapitals()
    {
        var item = PendingSpanBaseline.NewPendingSpanBaseline("code-361", "label-111");
        var view = PendingSpanBaselineView.ToPendingSpanBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingSpanBaselineView.ToPendingSpanBaselineViews(new List<PendingSpanBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingSpanBaselineViewShowsTheStatusInCapitals();
    }
}
