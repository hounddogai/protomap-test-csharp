namespace Acme.Workforce.Compliance.Tests.Exception.Proposal;

using Acme.Workforce.Compliance.Exception.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ExceptionOverrideEngineChecks
{
    public static void ExceptionOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionOverrideEngine.DefaultExceptionOverrideEngine();
        var item = ExceptionOverride.NewExceptionOverride("code-161", "name-834");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionOverrideEngineKeepsResultsWithinBounds();
    }
}
