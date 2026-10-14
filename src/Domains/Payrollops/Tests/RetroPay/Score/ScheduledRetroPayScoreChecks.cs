namespace Acme.Workforce.Payrollops.Tests.RetroPay.Score;

using Acme.Workforce.Payrollops.RetroPay.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class ScheduledRetroPayScoreChecks
{
    public static void ScheduledRetroPayScoreStartsInDraft()
    {
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("code-686", "title-741");
        Check.Equal(item.Status, ScheduledRetroPayScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledRetroPayScoreMovesToSuspended()
    {
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("code-686", "title-741");
        try
        {
            item.MoveTo(ScheduledRetroPayScoreStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledRetroPayScoreStatus.Suspended, "status");
    }

    public static void ScheduledRetroPayScoreRejectsMovesFromClosed()
    {
        var item = ScheduledRetroPayScore.NewScheduledRetroPayScore("code-686", "title-741");
        item.Status = ScheduledRetroPayScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledRetroPayScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledRetroPayScoreStartsInDraft();
        ScheduledRetroPayScoreMovesToSuspended();
        ScheduledRetroPayScoreRejectsMovesFromClosed();
    }
}
