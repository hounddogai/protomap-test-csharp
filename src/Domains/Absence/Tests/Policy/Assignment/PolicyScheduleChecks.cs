namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PolicyScheduleChecks
{
    public static void PolicyScheduleStartsInIdle()
    {
        var item = PolicySchedule.NewPolicySchedule("code-932", "title-618");
        Check.Equal(item.Status, PolicyScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyScheduleMovesToStopped()
    {
        var item = PolicySchedule.NewPolicySchedule("code-932", "title-618");
        try
        {
            item.MoveTo(PolicyScheduleStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyScheduleStatus.Stopped, "status");
    }

    public static void PolicyScheduleRejectsMovesFromStopped()
    {
        var item = PolicySchedule.NewPolicySchedule("code-932", "title-618");
        item.Status = PolicyScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(PolicyScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PolicyScheduleStartsInIdle();
        PolicyScheduleMovesToStopped();
        PolicyScheduleRejectsMovesFromStopped();
    }
}
