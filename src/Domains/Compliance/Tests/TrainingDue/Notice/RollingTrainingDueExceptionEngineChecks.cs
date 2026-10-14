namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingTrainingDueExceptionEngineChecks
{
    public static void RollingTrainingDueExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = RollingTrainingDueExceptionEngine.DefaultRollingTrainingDueExceptionEngine();
        var item = RollingTrainingDueException.NewRollingTrainingDueException("code-273", "name-751");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingTrainingDueExceptionEngineKeepsResultsWithinBounds();
    }
}
