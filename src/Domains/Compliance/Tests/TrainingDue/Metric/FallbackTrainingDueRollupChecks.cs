namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackTrainingDueRollupChecks
{
    public static void FallbackTrainingDueRollupStartsInRequested()
    {
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("code-850", "title-688");
        Check.Equal(item.Status, FallbackTrainingDueRollupStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackTrainingDueRollupMovesToGranted()
    {
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("code-850", "title-688");
        try
        {
            item.MoveTo(FallbackTrainingDueRollupStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackTrainingDueRollupStatus.Granted, "status");
    }

    public static void FallbackTrainingDueRollupRejectsMovesFromExpired()
    {
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("code-850", "title-688");
        item.Status = FallbackTrainingDueRollupStatus.Expired;
        Check.Fails(() => item.MoveTo(FallbackTrainingDueRollupStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FallbackTrainingDueRollupStartsInRequested();
        FallbackTrainingDueRollupMovesToGranted();
        FallbackTrainingDueRollupRejectsMovesFromExpired();
    }
}
