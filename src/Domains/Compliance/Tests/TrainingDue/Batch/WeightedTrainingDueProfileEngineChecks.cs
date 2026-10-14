namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedTrainingDueProfileEngineChecks
{
    public static void WeightedTrainingDueProfileEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedTrainingDueProfileEngine.DefaultWeightedTrainingDueProfileEngine(Flags.NewRegistry());
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("code-83", "title-448");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedTrainingDueProfileEngineKeepsResultsWithinBounds();
    }
}
