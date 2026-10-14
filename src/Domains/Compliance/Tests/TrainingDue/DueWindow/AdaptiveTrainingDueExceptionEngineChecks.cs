namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueExceptionEngineChecks
{
    public static void AdaptiveTrainingDueExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveTrainingDueExceptionEngine.DefaultAdaptiveTrainingDueExceptionEngine(Flags.NewRegistry());
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("code-245", "name-143");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveTrainingDueExceptionEngineKeepsResultsWithinBounds();
    }
}
