namespace Acme.Workforce.Payrollops.Tests.Deduction.Budget;

using Acme.Workforce.Payrollops.Deduction.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class DeductionReservationChecks
{
    public static void DeductionReservationStartsInIdle()
    {
        var item = DeductionReservation.NewDeductionReservation("code-676", "name-63");
        Check.Equal(item.Status, DeductionReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DeductionReservationMovesToWarming()
    {
        var item = DeductionReservation.NewDeductionReservation("code-676", "name-63");
        try
        {
            item.MoveTo(DeductionReservationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DeductionReservationStatus.Warming, "status");
    }

    public static void DeductionReservationRejectsMovesFromStopped()
    {
        var item = DeductionReservation.NewDeductionReservation("code-676", "name-63");
        item.Status = DeductionReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(DeductionReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        DeductionReservationStartsInIdle();
        DeductionReservationMovesToWarming();
        DeductionReservationRejectsMovesFromStopped();
    }
}
