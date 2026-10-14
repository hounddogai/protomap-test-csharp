namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class RollingRetroPayAssignmentChecks
{
    public static void RollingRetroPayAssignmentStartsInPlanned()
    {
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("code-80", "title-544");
        Check.Equal(item.Status, RollingRetroPayAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingRetroPayAssignmentMovesToScheduled()
    {
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("code-80", "title-544");
        try
        {
            item.MoveTo(RollingRetroPayAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingRetroPayAssignmentStatus.Scheduled, "status");
    }

    public static void RollingRetroPayAssignmentRejectsMovesFromCancelled()
    {
        var item = RollingRetroPayAssignment.NewRollingRetroPayAssignment("code-80", "title-544");
        item.Status = RollingRetroPayAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingRetroPayAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingRetroPayAssignmentStartsInPlanned();
        RollingRetroPayAssignmentMovesToScheduled();
        RollingRetroPayAssignmentRejectsMovesFromCancelled();
    }
}
