namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class DefaultExceptionAllocationEngineChecks
{
    public static void DefaultExceptionAllocationEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultExceptionAllocationEngine.DefaultDefaultExceptionAllocationEngine();
        var item = DefaultExceptionAllocation.NewDefaultExceptionAllocation("code-341", "name-148");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultExceptionAllocationEngineKeepsResultsWithinBounds();
    }
}
