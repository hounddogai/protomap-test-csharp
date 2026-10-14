namespace Acme.Workforce.Engagement.Tests.Recognition.Snapshot;

using Acme.Workforce.Engagement.Recognition.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyRecognitionBudgetChecks
{
    public static void LegacyRecognitionBudgetStartsInPlanned()
    {
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("code-878", "name-75");
        Check.Equal(item.Status, LegacyRecognitionBudgetStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyRecognitionBudgetMovesToCompleted()
    {
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("code-878", "name-75");
        try
        {
            item.MoveTo(LegacyRecognitionBudgetStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyRecognitionBudgetStatus.Completed, "status");
    }

    public static void LegacyRecognitionBudgetRejectsMovesFromCancelled()
    {
        var item = LegacyRecognitionBudget.NewLegacyRecognitionBudget("code-878", "name-75");
        item.Status = LegacyRecognitionBudgetStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyRecognitionBudgetStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyRecognitionBudgetStartsInPlanned();
        LegacyRecognitionBudgetMovesToCompleted();
        LegacyRecognitionBudgetRejectsMovesFromCancelled();
    }
}
