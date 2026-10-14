namespace Acme.Workforce.Compensation.Tests.EquityGrant.Schedule;

using Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class GlobalEquityGrantScheduleChecks
{
    public static void GlobalEquityGrantScheduleStartsInRequested()
    {
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("code-341", "name-420");
        Check.Equal(item.Status, GlobalEquityGrantScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalEquityGrantScheduleMovesToExpired()
    {
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("code-341", "name-420");
        try
        {
            item.MoveTo(GlobalEquityGrantScheduleStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalEquityGrantScheduleStatus.Expired, "status");
    }

    public static void GlobalEquityGrantScheduleRejectsMovesFromExpired()
    {
        var item = GlobalEquityGrantSchedule.NewGlobalEquityGrantSchedule("code-341", "name-420");
        item.Status = GlobalEquityGrantScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalEquityGrantScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalEquityGrantScheduleStartsInRequested();
        GlobalEquityGrantScheduleMovesToExpired();
        GlobalEquityGrantScheduleRejectsMovesFromExpired();
    }
}
