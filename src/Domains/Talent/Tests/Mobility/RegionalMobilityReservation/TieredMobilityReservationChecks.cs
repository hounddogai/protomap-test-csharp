namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class TieredMobilityReservationChecks
{
    public static void TieredMobilityReservationStartsInOpen()
    {
        var item = TieredMobilityReservation.NewTieredMobilityReservation("code-76", "name-656");
        Check.Equal(item.Status, TieredMobilityReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredMobilityReservationMovesToReopened()
    {
        var item = TieredMobilityReservation.NewTieredMobilityReservation("code-76", "name-656");
        try
        {
            item.MoveTo(TieredMobilityReservationStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredMobilityReservationStatus.Reopened, "status");
    }

    public static void TieredMobilityReservationRejectsMovesFromArchived()
    {
        var item = TieredMobilityReservation.NewTieredMobilityReservation("code-76", "name-656");
        item.Status = TieredMobilityReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(TieredMobilityReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        TieredMobilityReservationStartsInOpen();
        TieredMobilityReservationMovesToReopened();
        TieredMobilityReservationRejectsMovesFromArchived();
    }
}
