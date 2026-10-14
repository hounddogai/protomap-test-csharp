namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPoolThresholdChecks
{
    public static void ScheduledPoolThresholdStartsInNew()
    {
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("code-749", "title-268");
        Check.Equal(item.Status, ScheduledPoolThresholdStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledPoolThresholdMovesToRunning()
    {
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("code-749", "title-268");
        try
        {
            item.MoveTo(ScheduledPoolThresholdStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledPoolThresholdStatus.Running, "status");
    }

    public static void ScheduledPoolThresholdRejectsMovesFromFailed()
    {
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("code-749", "title-268");
        item.Status = ScheduledPoolThresholdStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledPoolThresholdStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledPoolThresholdStartsInNew();
        ScheduledPoolThresholdMovesToRunning();
        ScheduledPoolThresholdRejectsMovesFromFailed();
    }
}
