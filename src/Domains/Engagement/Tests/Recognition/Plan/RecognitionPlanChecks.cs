namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RecognitionPlanChecks
{
    public static void RecognitionPlanStartsInProposed()
    {
        var item = RecognitionPlan.NewRecognitionPlan("code-574", "title-883");
        Check.Equal(item.Status, RecognitionPlanStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionPlanMovesToRejected()
    {
        var item = RecognitionPlan.NewRecognitionPlan("code-574", "title-883");
        try
        {
            item.MoveTo(RecognitionPlanStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionPlanStatus.Rejected, "status");
    }

    public static void RecognitionPlanRejectsMovesFromWithdrawn()
    {
        var item = RecognitionPlan.NewRecognitionPlan("code-574", "title-883");
        item.Status = RecognitionPlanStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RecognitionPlanStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RecognitionPlanStartsInProposed();
        RecognitionPlanMovesToRejected();
        RecognitionPlanRejectsMovesFromWithdrawn();
    }
}
