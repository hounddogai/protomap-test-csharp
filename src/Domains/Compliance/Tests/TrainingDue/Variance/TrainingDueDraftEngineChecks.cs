namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueDraftEngineChecks
{
    public static void TrainingDueDraftEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueDraftEngine.DefaultTrainingDueDraftEngine();
        var item = TrainingDueDraft.NewTrainingDueDraft("code-518", "title-545");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueDraftEngineKeepsResultsWithinBounds();
    }
}
