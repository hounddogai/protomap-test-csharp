namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleReservationChecks
{
    public static void AdaptiveMeritCycleReservationStartsInDraft()
    {
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("code-287", "title-79");
        Check.Equal(item.Status, AdaptiveMeritCycleReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveMeritCycleReservationMovesToPending()
    {
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("code-287", "title-79");
        try
        {
            item.MoveTo(AdaptiveMeritCycleReservationStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveMeritCycleReservationStatus.Pending, "status");
    }

    public static void AdaptiveMeritCycleReservationRejectsMovesFromClosed()
    {
        var item = AdaptiveMeritCycleReservation.NewAdaptiveMeritCycleReservation("code-287", "title-79");
        item.Status = AdaptiveMeritCycleReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(AdaptiveMeritCycleReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        AdaptiveMeritCycleReservationStartsInDraft();
        AdaptiveMeritCycleReservationMovesToPending();
        AdaptiveMeritCycleReservationRejectsMovesFromClosed();
    }
}
