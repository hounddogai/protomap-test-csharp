namespace Acme.Workforce.Talent.Tests.Mobility.Tier;

using Acme.Workforce.Talent.Mobility.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualMobilityReservationChecks
{
    public static void ManualMobilityReservationStartsInIdle()
    {
        var item = ManualMobilityReservation.NewManualMobilityReservation("code-634", "title-31");
        Check.Equal(item.Status, ManualMobilityReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualMobilityReservationMovesToReady()
    {
        var item = ManualMobilityReservation.NewManualMobilityReservation("code-634", "title-31");
        try
        {
            item.MoveTo(ManualMobilityReservationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualMobilityReservationStatus.Ready, "status");
    }

    public static void ManualMobilityReservationRejectsMovesFromStopped()
    {
        var item = ManualMobilityReservation.NewManualMobilityReservation("code-634", "title-31");
        item.Status = ManualMobilityReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualMobilityReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualMobilityReservationStartsInIdle();
        ManualMobilityReservationMovesToReady();
        ManualMobilityReservationRejectsMovesFromStopped();
    }
}
