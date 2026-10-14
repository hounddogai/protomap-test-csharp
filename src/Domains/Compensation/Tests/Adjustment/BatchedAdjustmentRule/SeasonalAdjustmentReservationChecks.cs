namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReservationChecks
{
    public static void SeasonalAdjustmentReservationStartsInDraft()
    {
        var item = SeasonalAdjustmentReservation.NewSeasonalAdjustmentReservation("code-664", "label-850");
        Check.Equal(item.Status, SeasonalAdjustmentReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAdjustmentReservationMovesToSuspended()
    {
        var item = SeasonalAdjustmentReservation.NewSeasonalAdjustmentReservation("code-664", "label-850");
        try
        {
            item.MoveTo(SeasonalAdjustmentReservationStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAdjustmentReservationStatus.Suspended, "status");
    }

    public static void SeasonalAdjustmentReservationRejectsMovesFromClosed()
    {
        var item = SeasonalAdjustmentReservation.NewSeasonalAdjustmentReservation("code-664", "label-850");
        item.Status = SeasonalAdjustmentReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(SeasonalAdjustmentReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReservationStartsInDraft();
        SeasonalAdjustmentReservationMovesToSuspended();
        SeasonalAdjustmentReservationRejectsMovesFromClosed();
    }
}
