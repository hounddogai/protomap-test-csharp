namespace Acme.Workforce.Absence.Tests.ReturnPlan.Window;

using Acme.Workforce.Absence.ReturnPlan.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PendingReturnPlanWindowChecks
{
    public static void PendingReturnPlanWindowStartsInIdle()
    {
        var item = PendingReturnPlanWindow.NewPendingReturnPlanWindow("code-463", "label-771");
        Check.Equal(item.Status, PendingReturnPlanWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingReturnPlanWindowMovesToStopped()
    {
        var item = PendingReturnPlanWindow.NewPendingReturnPlanWindow("code-463", "label-771");
        try
        {
            item.MoveTo(PendingReturnPlanWindowStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingReturnPlanWindowStatus.Stopped, "status");
    }

    public static void PendingReturnPlanWindowRejectsMovesFromStopped()
    {
        var item = PendingReturnPlanWindow.NewPendingReturnPlanWindow("code-463", "label-771");
        item.Status = PendingReturnPlanWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingReturnPlanWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingReturnPlanWindowStartsInIdle();
        PendingReturnPlanWindowMovesToStopped();
        PendingReturnPlanWindowRejectsMovesFromStopped();
    }
}
