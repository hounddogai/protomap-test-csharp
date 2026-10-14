namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ExceptionSummaryEngineChecks
{
    public static void ExceptionSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionSummaryEngine.DefaultExceptionSummaryEngine();
        var item = ExceptionSummary.NewExceptionSummary("code-451", "name-529");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionSummaryEngineKeepsResultsWithinBounds();
    }
}
