namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PoolReservationChecks
{
    public static void PoolReservationStartsInPlanned()
    {
        var item = PoolReservation.NewPoolReservation("code-725", "label-924");
        Check.Equal(item.Status, PoolReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolReservationMovesToInProgress()
    {
        var item = PoolReservation.NewPoolReservation("code-725", "label-924");
        try
        {
            item.MoveTo(PoolReservationStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolReservationStatus.InProgress, "status");
    }

    public static void PoolReservationRejectsMovesFromCancelled()
    {
        var item = PoolReservation.NewPoolReservation("code-725", "label-924");
        item.Status = PoolReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PoolReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PoolReservationStartsInPlanned();
        PoolReservationMovesToInProgress();
        PoolReservationRejectsMovesFromCancelled();
    }
}
