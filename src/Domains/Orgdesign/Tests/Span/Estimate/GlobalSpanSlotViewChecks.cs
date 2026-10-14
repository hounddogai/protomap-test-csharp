namespace Acme.Workforce.Orgdesign.Tests.Span.Estimate;

using Acme.Workforce.Orgdesign.Span.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalSpanSlotViewChecks
{
    public static void GlobalSpanSlotViewShowsTheStatusInCapitals()
    {
        var item = GlobalSpanSlot.NewGlobalSpanSlot("code-324", "title-867");
        var view = GlobalSpanSlotView.ToGlobalSpanSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalSpanSlotView.ToGlobalSpanSlotViews(new List<GlobalSpanSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalSpanSlotViewShowsTheStatusInCapitals();
    }
}
