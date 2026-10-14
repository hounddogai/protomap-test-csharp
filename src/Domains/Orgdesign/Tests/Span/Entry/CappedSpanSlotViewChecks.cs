namespace Acme.Workforce.Orgdesign.Tests.Span.Entry;

using Acme.Workforce.Orgdesign.Span.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class CappedSpanSlotViewChecks
{
    public static void CappedSpanSlotViewShowsTheStatusInCapitals()
    {
        var item = CappedSpanSlot.NewCappedSpanSlot("code-890", "label-457");
        var view = CappedSpanSlotView.ToCappedSpanSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedSpanSlotView.ToCappedSpanSlotViews(new List<CappedSpanSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedSpanSlotViewShowsTheStatusInCapitals();
    }
}
