namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class SeasonalExceptionManifestEngineChecks
{
    public static void SeasonalExceptionManifestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalExceptionManifestEngine.DefaultSeasonalExceptionManifestEngine();
        var item = SeasonalExceptionManifest.NewSeasonalExceptionManifest("code-35", "label-841");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalExceptionManifestEngineKeepsResultsWithinBounds();
    }
}
