namespace Acme.Workforce.Orgdesign.Tests.Span.Policy;

using Acme.Workforce.Orgdesign.Span.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class CappedSpanThresholdViewChecks
{
    public static void CappedSpanThresholdViewShowsTheStatusInCapitals()
    {
        var item = CappedSpanThreshold.NewCappedSpanThreshold("code-474", "title-349");
        var view = CappedSpanThresholdView.ToCappedSpanThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedSpanThresholdView.ToCappedSpanThresholdViews(new List<CappedSpanThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedSpanThresholdViewShowsTheStatusInCapitals();
    }
}
