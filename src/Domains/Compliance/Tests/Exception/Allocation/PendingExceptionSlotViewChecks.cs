namespace Acme.Workforce.Compliance.Tests.Exception.Allocation;

using Acme.Workforce.Compliance.Exception.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class PendingExceptionSlotViewChecks
{
    public static void PendingExceptionSlotViewShowsTheStatusInCapitals()
    {
        var item = PendingExceptionSlot.NewPendingExceptionSlot("code-412", "title-245");
        var view = PendingExceptionSlotView.ToPendingExceptionSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingExceptionSlotView.ToPendingExceptionSlotViews(new List<PendingExceptionSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingExceptionSlotViewShowsTheStatusInCapitals();
    }
}
