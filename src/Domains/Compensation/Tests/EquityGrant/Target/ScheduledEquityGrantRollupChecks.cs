namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledEquityGrantRollupChecks
{
    public static void ScheduledEquityGrantRollupStartsInNew()
    {
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("code-697", "label-663");
        Check.Equal(item.Status, ScheduledEquityGrantRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledEquityGrantRollupMovesToRunning()
    {
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("code-697", "label-663");
        try
        {
            item.MoveTo(ScheduledEquityGrantRollupStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledEquityGrantRollupStatus.Running, "status");
    }

    public static void ScheduledEquityGrantRollupRejectsMovesFromFailed()
    {
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("code-697", "label-663");
        item.Status = ScheduledEquityGrantRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledEquityGrantRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantRollupStartsInNew();
        ScheduledEquityGrantRollupMovesToRunning();
        ScheduledEquityGrantRollupRejectsMovesFromFailed();
    }
}
