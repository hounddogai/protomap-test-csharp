namespace Acme.Workforce.Compensation.Tests.Adjustment.Notice;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class AdjustmentReservationChecks
{
    public static void AdjustmentReservationStartsInOpen()
    {
        var item = AdjustmentReservation.NewAdjustmentReservation("code-198", "name-755");
        Check.Equal(item.Status, AdjustmentReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentReservationMovesToResolved()
    {
        var item = AdjustmentReservation.NewAdjustmentReservation("code-198", "name-755");
        try
        {
            item.MoveTo(AdjustmentReservationStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentReservationStatus.Resolved, "status");
    }

    public static void AdjustmentReservationRejectsMovesFromArchived()
    {
        var item = AdjustmentReservation.NewAdjustmentReservation("code-198", "name-755");
        item.Status = AdjustmentReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(AdjustmentReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdjustmentReservationStartsInOpen();
        AdjustmentReservationMovesToResolved();
        AdjustmentReservationRejectsMovesFromArchived();
    }
}
