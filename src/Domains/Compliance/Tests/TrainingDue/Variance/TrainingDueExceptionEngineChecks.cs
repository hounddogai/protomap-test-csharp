namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueExceptionEngineChecks
{
    public static void TrainingDueExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = TrainingDueExceptionEngine.DefaultTrainingDueExceptionEngine(Flags.NewRegistry());
        var item = TrainingDueException.NewTrainingDueException("code-327", "title-464");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TrainingDueExceptionEngineKeepsResultsWithinBounds();
    }
}
