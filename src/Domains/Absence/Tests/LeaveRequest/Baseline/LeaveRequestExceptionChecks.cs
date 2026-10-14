namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestExceptionChecks
{
    public static void LeaveRequestExceptionStartsInDraft()
    {
        var item = LeaveRequestException.NewLeaveRequestException("code-286", "label-81");
        Check.Equal(item.Status, LeaveRequestExceptionStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestExceptionMovesToActive()
    {
        var item = LeaveRequestException.NewLeaveRequestException("code-286", "label-81");
        try
        {
            item.MoveTo(LeaveRequestExceptionStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestExceptionStatus.Active, "status");
    }

    public static void LeaveRequestExceptionRejectsMovesFromClosed()
    {
        var item = LeaveRequestException.NewLeaveRequestException("code-286", "label-81");
        item.Status = LeaveRequestExceptionStatus.Closed;
        Check.Fails(() => item.MoveTo(LeaveRequestExceptionStatus.Draft), "move");
    }

    public static void RunAll()
    {
        LeaveRequestExceptionStartsInDraft();
        LeaveRequestExceptionMovesToActive();
        LeaveRequestExceptionRejectsMovesFromClosed();
    }
}
