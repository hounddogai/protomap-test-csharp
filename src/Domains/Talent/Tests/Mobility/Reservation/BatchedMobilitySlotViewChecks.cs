namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BatchedMobilitySlotViewChecks
{
    public static void BatchedMobilitySlotViewShowsTheStatusInCapitals()
    {
        var item = BatchedMobilitySlot.NewBatchedMobilitySlot("code-67", "title-523");
        var view = BatchedMobilitySlotView.ToBatchedMobilitySlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedMobilitySlotView.ToBatchedMobilitySlotViews(new List<BatchedMobilitySlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedMobilitySlotViewShowsTheStatusInCapitals();
    }
}
