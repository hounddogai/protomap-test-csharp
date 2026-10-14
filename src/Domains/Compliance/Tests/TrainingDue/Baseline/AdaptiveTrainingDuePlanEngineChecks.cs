namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveTrainingDuePlanEngineChecks
{
    public static void AdaptiveTrainingDuePlanEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTrainingDuePlanEngine.DefaultAdaptiveTrainingDuePlanEngine(Flags.NewRegistry());
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("code-987", "label-708");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDuePlanEngineKeepsResultsWithinBounds();
    }
}
