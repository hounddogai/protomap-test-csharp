namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ManualLeaveRequestScoreChecks
{
    public static void ManualLeaveRequestScoreStartsInRequested()
    {
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("code-475", "name-841");
        Check.Equal(item.Status, ManualLeaveRequestScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualLeaveRequestScoreMovesToRevoked()
    {
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("code-475", "name-841");
        try
        {
            item.MoveTo(ManualLeaveRequestScoreStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualLeaveRequestScoreStatus.Revoked, "status");
    }

    public static void ManualLeaveRequestScoreRejectsMovesFromExpired()
    {
        var item = ManualLeaveRequestScore.NewManualLeaveRequestScore("code-475", "name-841");
        item.Status = ManualLeaveRequestScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualLeaveRequestScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualLeaveRequestScoreStartsInRequested();
        ManualLeaveRequestScoreMovesToRevoked();
        ManualLeaveRequestScoreRejectsMovesFromExpired();
    }
}
