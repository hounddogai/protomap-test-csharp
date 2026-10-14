namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class WeightedEngagementReservationChecks
{
    public static void WeightedEngagementReservationStartsInOpen()
    {
        var item = WeightedEngagementReservation.NewWeightedEngagementReservation("code-721", "label-107");
        Check.Equal(item.Status, WeightedEngagementReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedEngagementReservationMovesToArchived()
    {
        var item = WeightedEngagementReservation.NewWeightedEngagementReservation("code-721", "label-107");
        try
        {
            item.MoveTo(WeightedEngagementReservationStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedEngagementReservationStatus.Archived, "status");
    }

    public static void WeightedEngagementReservationRejectsMovesFromArchived()
    {
        var item = WeightedEngagementReservation.NewWeightedEngagementReservation("code-721", "label-107");
        item.Status = WeightedEngagementReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedEngagementReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedEngagementReservationStartsInOpen();
        WeightedEngagementReservationMovesToArchived();
        WeightedEngagementReservationRejectsMovesFromArchived();
    }
}
