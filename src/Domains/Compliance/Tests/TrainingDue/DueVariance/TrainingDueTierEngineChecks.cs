namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueTierEngineChecks
{
    public static void TrainingDueTierEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueTierEngine.DefaultTrainingDueTierEngine();
        var item = TrainingDueTier.NewTrainingDueTier("code-697", "label-128");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueTierEngineKeepsResultsWithinBounds();
    }
}
