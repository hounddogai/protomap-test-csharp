namespace Acme.Workforce.Absence.Tests.LeaveRequest.Batch;

using Acme.Workforce.Absence.LeaveRequest.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class LeaveRequestSegmentChecks
{
    public static void LeaveRequestSegmentStartsInRequested()
    {
        var item = LeaveRequestSegment.NewLeaveRequestSegment("code-92", "title-733");
        Check.Equal(item.Status, LeaveRequestSegmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestSegmentMovesToExpired()
    {
        var item = LeaveRequestSegment.NewLeaveRequestSegment("code-92", "title-733");
        try
        {
            item.MoveTo(LeaveRequestSegmentStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestSegmentStatus.Expired, "status");
    }

    public static void LeaveRequestSegmentRejectsMovesFromExpired()
    {
        var item = LeaveRequestSegment.NewLeaveRequestSegment("code-92", "title-733");
        item.Status = LeaveRequestSegmentStatus.Expired;
        Check.Fails(() => item.MoveTo(LeaveRequestSegmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LeaveRequestSegmentStartsInRequested();
        LeaveRequestSegmentMovesToExpired();
        LeaveRequestSegmentRejectsMovesFromExpired();
    }
}
