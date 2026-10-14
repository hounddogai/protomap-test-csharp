namespace Acme.Workforce.Absence.Tests.Accrual.OverrideItem;

using Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class LenientAccrualAssignmentChecks
{
    public static void LenientAccrualAssignmentStartsInIdle()
    {
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("code-301", "label-784");
        Check.Equal(item.Status, LenientAccrualAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientAccrualAssignmentMovesToWarming()
    {
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("code-301", "label-784");
        try
        {
            item.MoveTo(LenientAccrualAssignmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientAccrualAssignmentStatus.Warming, "status");
    }

    public static void LenientAccrualAssignmentRejectsMovesFromStopped()
    {
        var item = LenientAccrualAssignment.NewLenientAccrualAssignment("code-301", "label-784");
        item.Status = LenientAccrualAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientAccrualAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientAccrualAssignmentStartsInIdle();
        LenientAccrualAssignmentMovesToWarming();
        LenientAccrualAssignmentRejectsMovesFromStopped();
    }
}
