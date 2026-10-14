namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestTemplateChecks
{
    public static void LeaveRequestTemplateStartsInRequested()
    {
        var item = LeaveRequestTemplate.NewLeaveRequestTemplate("code-235", "name-118");
        Check.Equal(item.Status, LeaveRequestTemplateStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestTemplateMovesToExpired()
    {
        var item = LeaveRequestTemplate.NewLeaveRequestTemplate("code-235", "name-118");
        try
        {
            item.MoveTo(LeaveRequestTemplateStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestTemplateStatus.Expired, "status");
    }

    public static void LeaveRequestTemplateRejectsMovesFromExpired()
    {
        var item = LeaveRequestTemplate.NewLeaveRequestTemplate("code-235", "name-118");
        item.Status = LeaveRequestTemplateStatus.Expired;
        Check.Fails(() => item.MoveTo(LeaveRequestTemplateStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LeaveRequestTemplateStartsInRequested();
        LeaveRequestTemplateMovesToExpired();
        LeaveRequestTemplateRejectsMovesFromExpired();
    }
}
