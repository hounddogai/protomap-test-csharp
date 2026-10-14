namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueReviewEngineChecks
{
    public static void TrainingDueReviewEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueReviewEngine.DefaultTrainingDueReviewEngine();
        var item = TrainingDueReview.NewTrainingDueReview("code-434", "label-533");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueReviewEngineKeepsResultsWithinBounds();
    }
}
