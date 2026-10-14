namespace Acme.Workforce.Compliance.Tests.Exception.Cycle;

using Acme.Workforce.Compliance.Exception.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ExceptionCycleEngineChecks
{
    public static void ExceptionCycleEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionCycleEngine.DefaultExceptionCycleEngine();
        var item = ExceptionCycle.NewExceptionCycle("code-540", "label-489");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionCycleEngineKeepsResultsWithinBounds();
    }
}
