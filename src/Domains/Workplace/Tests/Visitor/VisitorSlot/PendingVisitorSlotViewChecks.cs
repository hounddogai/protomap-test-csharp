namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorSlot;

using Acme.Workforce.Workplace.Visitor.VisitorSlot;

/// <summary>Checks of visitor slot code, run by the project's runner.</summary>
public static class PendingVisitorSlotViewChecks
{
    public static void PendingVisitorSlotViewShowsTheStatusInCapitals()
    {
        var item = PendingVisitorSlot.NewPendingVisitorSlot("code-319", "title-722");
        var view = PendingVisitorSlotView.ToPendingVisitorSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingVisitorSlotView.ToPendingVisitorSlotViews(new List<PendingVisitorSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingVisitorSlotViewShowsTheStatusInCapitals();
    }
}
