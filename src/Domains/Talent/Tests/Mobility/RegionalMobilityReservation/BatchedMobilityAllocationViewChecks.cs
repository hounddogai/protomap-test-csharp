namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class BatchedMobilityAllocationViewChecks
{
    public static void BatchedMobilityAllocationViewShowsTheStatusInCapitals()
    {
        var item = BatchedMobilityAllocation.NewBatchedMobilityAllocation("code-805", "title-322");
        var view = BatchedMobilityAllocationView.ToBatchedMobilityAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedMobilityAllocationView.ToBatchedMobilityAllocationViews(new List<BatchedMobilityAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedMobilityAllocationViewShowsTheStatusInCapitals();
    }
}
