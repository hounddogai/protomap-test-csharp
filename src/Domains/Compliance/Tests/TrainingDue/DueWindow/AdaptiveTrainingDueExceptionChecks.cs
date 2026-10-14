namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueExceptionChecks
{
    public static void AdaptiveTrainingDueExceptionStartsInPlanned()
    {
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("code-245", "name-143");
        Check.Equal(item.Status, AdaptiveTrainingDueExceptionStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveTrainingDueExceptionMovesToScheduled()
    {
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("code-245", "name-143");
        try
        {
            item.MoveTo(AdaptiveTrainingDueExceptionStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveTrainingDueExceptionStatus.Scheduled, "status");
    }

    public static void AdaptiveTrainingDueExceptionRejectsMovesFromCancelled()
    {
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("code-245", "name-143");
        item.Status = AdaptiveTrainingDueExceptionStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdaptiveTrainingDueExceptionStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueExceptionStartsInPlanned();
        AdaptiveTrainingDueExceptionMovesToScheduled();
        AdaptiveTrainingDueExceptionRejectsMovesFromCancelled();
    }
}
