namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class RollingLeaveRequestPreferenceChecks
{
    public static void RollingLeaveRequestPreferenceStartsInNew()
    {
        var item = RollingLeaveRequestPreference.NewRollingLeaveRequestPreference("code-768", "label-356");
        Check.Equal(item.Status, RollingLeaveRequestPreferenceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingLeaveRequestPreferenceMovesToQueued()
    {
        var item = RollingLeaveRequestPreference.NewRollingLeaveRequestPreference("code-768", "label-356");
        try
        {
            item.MoveTo(RollingLeaveRequestPreferenceStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingLeaveRequestPreferenceStatus.Queued, "status");
    }

    public static void RollingLeaveRequestPreferenceRejectsMovesFromFailed()
    {
        var item = RollingLeaveRequestPreference.NewRollingLeaveRequestPreference("code-768", "label-356");
        item.Status = RollingLeaveRequestPreferenceStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingLeaveRequestPreferenceStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingLeaveRequestPreferenceStartsInNew();
        RollingLeaveRequestPreferenceMovesToQueued();
        RollingLeaveRequestPreferenceRejectsMovesFromFailed();
    }
}
