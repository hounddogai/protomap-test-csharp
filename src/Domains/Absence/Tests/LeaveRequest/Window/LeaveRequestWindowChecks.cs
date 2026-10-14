namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LeaveRequestWindowChecks
{
    public static void LeaveRequestWindowStartsInCreated()
    {
        var item = LeaveRequestWindow.NewLeaveRequestWindow("code-566", "name-523");
        Check.Equal(item.Status, LeaveRequestWindowStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestWindowMovesToPublished()
    {
        var item = LeaveRequestWindow.NewLeaveRequestWindow("code-566", "name-523");
        try
        {
            item.MoveTo(LeaveRequestWindowStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestWindowStatus.Published, "status");
    }

    public static void LeaveRequestWindowRejectsMovesFromRetired()
    {
        var item = LeaveRequestWindow.NewLeaveRequestWindow("code-566", "name-523");
        item.Status = LeaveRequestWindowStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestWindowStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestWindowStartsInCreated();
        LeaveRequestWindowMovesToPublished();
        LeaveRequestWindowRejectsMovesFromRetired();
    }
}
