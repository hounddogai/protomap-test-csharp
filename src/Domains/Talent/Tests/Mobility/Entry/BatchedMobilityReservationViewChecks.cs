namespace Acme.Workforce.Talent.Tests.Mobility.Entry;

using Acme.Workforce.Talent.Mobility.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class BatchedMobilityReservationViewChecks
{
    public static void BatchedMobilityReservationViewShowsTheStatusInCapitals()
    {
        var item = BatchedMobilityReservation.NewBatchedMobilityReservation("code-42", "title-255");
        var view = BatchedMobilityReservationView.ToBatchedMobilityReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedMobilityReservationView.ToBatchedMobilityReservationViews(new List<BatchedMobilityReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedMobilityReservationViewShowsTheStatusInCapitals();
    }
}
