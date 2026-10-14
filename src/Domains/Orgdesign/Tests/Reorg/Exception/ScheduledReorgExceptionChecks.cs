namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledReorgExceptionChecks
{
    public static void ScheduledReorgExceptionStartsInPlanned()
    {
        var item = ScheduledReorgException.NewScheduledReorgException("code-648", "name-485");
        Check.Equal(item.Status, ScheduledReorgExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledReorgExceptionMovesToCompleted()
    {
        var item = ScheduledReorgException.NewScheduledReorgException("code-648", "name-485");
        try
        {
            item.MoveTo(ScheduledReorgExceptionStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledReorgExceptionStatus.Completed, "status");
    }

    public static void ScheduledReorgExceptionRejectsMovesFromCancelled()
    {
        var item = ScheduledReorgException.NewScheduledReorgException("code-648", "name-485");
        item.Status = ScheduledReorgExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledReorgExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledReorgExceptionStartsInPlanned();
        ScheduledReorgExceptionMovesToCompleted();
        ScheduledReorgExceptionRejectsMovesFromCancelled();
    }
}
