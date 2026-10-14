namespace Acme.Workforce.Workplace.Tests.Desk.Slot;

using Acme.Workforce.Workplace.Desk.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class DeskSlotViewChecks
{
    public static void DeskSlotViewShowsTheStatusInCapitals()
    {
        var item = DeskSlot.NewDeskSlot("code-151", "label-66");
        var view = DeskSlotView.ToDeskSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeskSlotView.ToDeskSlotViews(new List<DeskSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeskSlotViewShowsTheStatusInCapitals();
    }
}
