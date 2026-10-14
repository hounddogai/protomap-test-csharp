namespace Acme.Workforce.Orgdesign.Tests.Span.Metric;

using Acme.Workforce.Orgdesign.Span.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LegacySpanAdjustmentViewChecks
{
    public static void LegacySpanAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = LegacySpanAdjustment.NewLegacySpanAdjustment("code-325", "name-395");
        var view = LegacySpanAdjustmentView.ToLegacySpanAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacySpanAdjustmentView.ToLegacySpanAdjustmentViews(new List<LegacySpanAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacySpanAdjustmentViewShowsTheStatusInCapitals();
    }
}
