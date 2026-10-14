namespace Acme.Workforce.Payrollops.Tests.PayRun.Cycle;

using Acme.Workforce.Payrollops.PayRun.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LegacyPayRunAssignmentChecks
{
    public static void LegacyPayRunAssignmentStartsInIdle()
    {
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("code-250", "name-109");
        Check.Equal(item.Status, LegacyPayRunAssignmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPayRunAssignmentMovesToStopped()
    {
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("code-250", "name-109");
        try
        {
            item.MoveTo(LegacyPayRunAssignmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPayRunAssignmentStatus.Stopped, "status");
    }

    public static void LegacyPayRunAssignmentRejectsMovesFromStopped()
    {
        var item = LegacyPayRunAssignment.NewLegacyPayRunAssignment("code-250", "name-109");
        item.Status = LegacyPayRunAssignmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyPayRunAssignmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyPayRunAssignmentStartsInIdle();
        LegacyPayRunAssignmentMovesToStopped();
        LegacyPayRunAssignmentRejectsMovesFromStopped();
    }
}
