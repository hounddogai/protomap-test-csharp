namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LeaveRequestQuotaChecks
{
    public static void LeaveRequestQuotaStartsInCreated()
    {
        var item = LeaveRequestQuota.NewLeaveRequestQuota("code-252", "name-154");
        Check.Equal(item.Status, LeaveRequestQuotaStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestQuotaMovesToRetired()
    {
        var item = LeaveRequestQuota.NewLeaveRequestQuota("code-252", "name-154");
        try
        {
            item.MoveTo(LeaveRequestQuotaStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestQuotaStatus.Retired, "status");
    }

    public static void LeaveRequestQuotaRejectsMovesFromRetired()
    {
        var item = LeaveRequestQuota.NewLeaveRequestQuota("code-252", "name-154");
        item.Status = LeaveRequestQuotaStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestQuotaStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestQuotaStartsInCreated();
        LeaveRequestQuotaMovesToRetired();
        LeaveRequestQuotaRejectsMovesFromRetired();
    }
}
