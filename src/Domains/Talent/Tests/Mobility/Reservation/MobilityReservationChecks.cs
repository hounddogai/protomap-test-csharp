namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityReservationChecks
{
    public static void MobilityReservationStartsInOpen()
    {
        var item = MobilityReservation.NewMobilityReservation("code-842", "label-741");
        Check.Equal(item.Status, MobilityReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityReservationMovesToInReview()
    {
        var item = MobilityReservation.NewMobilityReservation("code-842", "label-741");
        try
        {
            item.MoveTo(MobilityReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityReservationStatus.InReview, "status");
    }

    public static void MobilityReservationRejectsMovesFromArchived()
    {
        var item = MobilityReservation.NewMobilityReservation("code-842", "label-741");
        item.Status = MobilityReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(MobilityReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        MobilityReservationStartsInOpen();
        MobilityReservationMovesToInReview();
        MobilityReservationRejectsMovesFromArchived();
    }
}
