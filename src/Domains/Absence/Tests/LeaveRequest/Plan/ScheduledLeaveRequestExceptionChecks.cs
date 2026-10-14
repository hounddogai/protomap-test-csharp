namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestExceptionChecks
{
    public static void ScheduledLeaveRequestExceptionStartsInCreated()
    {
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("code-174", "label-660");
        Check.Equal(item.Status, ScheduledLeaveRequestExceptionStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestExceptionMovesToRetired()
    {
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("code-174", "label-660");
        try
        {
            item.MoveTo(ScheduledLeaveRequestExceptionStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestExceptionStatus.Retired, "status");
    }

    public static void ScheduledLeaveRequestExceptionRejectsMovesFromRetired()
    {
        var item = ScheduledLeaveRequestException.NewScheduledLeaveRequestException("code-174", "label-660");
        item.Status = ScheduledLeaveRequestExceptionStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestExceptionStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestExceptionStartsInCreated();
        ScheduledLeaveRequestExceptionMovesToRetired();
        ScheduledLeaveRequestExceptionRejectsMovesFromRetired();
    }
}
