namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class LegacyTrainingDueVarianceEngineChecks
{
    public static void LegacyTrainingDueVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTrainingDueVarianceEngine.DefaultLegacyTrainingDueVarianceEngine();
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("code-870", "label-513");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTrainingDueVarianceEngineKeepsResultsWithinBounds();
    }
}
