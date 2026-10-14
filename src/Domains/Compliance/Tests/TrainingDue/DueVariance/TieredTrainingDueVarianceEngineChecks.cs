namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TieredTrainingDueVarianceEngineChecks
{
    public static void TieredTrainingDueVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = TieredTrainingDueVarianceEngine.DefaultTieredTrainingDueVarianceEngine(Flags.NewRegistry());
        var item = TieredTrainingDueVariance.NewTieredTrainingDueVariance("code-62", "title-708");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredTrainingDueVarianceEngineKeepsResultsWithinBounds();
    }
}
