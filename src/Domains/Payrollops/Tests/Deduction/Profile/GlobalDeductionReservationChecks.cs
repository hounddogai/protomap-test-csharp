namespace Acme.Workforce.Payrollops.Tests.Deduction.Profile;

using Acme.Workforce.Payrollops.Deduction.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GlobalDeductionReservationChecks
{
    public static void GlobalDeductionReservationStartsInIdle()
    {
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("code-479", "title-418");
        Check.Equal(item.Status, GlobalDeductionReservationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalDeductionReservationMovesToReady()
    {
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("code-479", "title-418");
        try
        {
            item.MoveTo(GlobalDeductionReservationStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalDeductionReservationStatus.Ready, "status");
    }

    public static void GlobalDeductionReservationRejectsMovesFromStopped()
    {
        var item = GlobalDeductionReservation.NewGlobalDeductionReservation("code-479", "title-418");
        item.Status = GlobalDeductionReservationStatus.Stopped;
        Check.Fails(() => item.MoveTo(GlobalDeductionReservationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        GlobalDeductionReservationStartsInIdle();
        GlobalDeductionReservationMovesToReady();
        GlobalDeductionReservationRejectsMovesFromStopped();
    }
}
