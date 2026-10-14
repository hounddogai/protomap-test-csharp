namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class FallbackRecognitionPlanChecks
{
    public static void FallbackRecognitionPlanStartsInPlanned()
    {
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("code-325", "label-769");
        Check.Equal(item.Status, FallbackRecognitionPlanStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackRecognitionPlanMovesToCompleted()
    {
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("code-325", "label-769");
        try
        {
            item.MoveTo(FallbackRecognitionPlanStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackRecognitionPlanStatus.Completed, "status");
    }

    public static void FallbackRecognitionPlanRejectsMovesFromCancelled()
    {
        var item = FallbackRecognitionPlan.NewFallbackRecognitionPlan("code-325", "label-769");
        item.Status = FallbackRecognitionPlanStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackRecognitionPlanStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackRecognitionPlanStartsInPlanned();
        FallbackRecognitionPlanMovesToCompleted();
        FallbackRecognitionPlanRejectsMovesFromCancelled();
    }
}
