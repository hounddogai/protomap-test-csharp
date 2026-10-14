namespace Acme.Workforce.Orgdesign.Tests.Span.SpanReview;

using Acme.Workforce.Orgdesign.Span.SpanReview;

/// <summary>Checks of span review code, run by the project's runner.</summary>
public static class ManualSpanPlanViewChecks
{
    public static void ManualSpanPlanViewShowsTheStatusInCapitals()
    {
        var item = ManualSpanPlan.NewManualSpanPlan("code-511", "name-23");
        var view = ManualSpanPlanView.ToManualSpanPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualSpanPlanView.ToManualSpanPlanViews(new List<ManualSpanPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualSpanPlanViewShowsTheStatusInCapitals();
    }
}
