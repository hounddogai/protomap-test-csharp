namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class HeadcountSlotViewChecks
{
    public static void HeadcountSlotViewShowsTheStatusInCapitals()
    {
        var item = HeadcountSlot.NewHeadcountSlot("code-7", "title-318");
        var view = HeadcountSlotView.ToHeadcountSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountSlotView.ToHeadcountSlotViews(new List<HeadcountSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountSlotViewShowsTheStatusInCapitals();
    }
}
