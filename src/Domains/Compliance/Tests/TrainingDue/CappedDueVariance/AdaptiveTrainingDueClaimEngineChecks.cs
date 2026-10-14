namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueClaimEngineChecks
{
    public static void AdaptiveTrainingDueClaimEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTrainingDueClaimEngine.DefaultAdaptiveTrainingDueClaimEngine(Flags.NewRegistry());
        var item = AdaptiveTrainingDueClaim.NewAdaptiveTrainingDueClaim("code-2", "title-354");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueClaimEngineKeepsResultsWithinBounds();
    }
}
