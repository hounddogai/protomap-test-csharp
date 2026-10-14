namespace Acme.Workforce.Assets.Tests.Refresh.Schedule;

using Acme.Workforce.Assets.Refresh.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RefreshScheduleChecks
{
    public static void RefreshScheduleStartsInPlanned()
    {
        var item = RefreshSchedule.NewRefreshSchedule("code-942", "title-275");
        Check.Equal(item.Status, RefreshScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshScheduleMovesToInProgress()
    {
        var item = RefreshSchedule.NewRefreshSchedule("code-942", "title-275");
        try
        {
            item.MoveTo(RefreshScheduleStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshScheduleStatus.InProgress, "status");
    }

    public static void RefreshScheduleRejectsMovesFromCancelled()
    {
        var item = RefreshSchedule.NewRefreshSchedule("code-942", "title-275");
        item.Status = RefreshScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshScheduleStartsInPlanned();
        RefreshScheduleMovesToInProgress();
        RefreshScheduleRejectsMovesFromCancelled();
    }
}
