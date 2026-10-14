namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionManifest;

using Acme.Workforce.Compliance.Exception.ExceptionManifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception manifest code, run by the project's runner.</summary>
public static class ManualExceptionManifestEngineChecks
{
    public static void ManualExceptionManifestEngineKeepsResultsWithinBounds()
    {
        var engine = ManualExceptionManifestEngine.DefaultManualExceptionManifestEngine(Flags.NewRegistry());
        var item = ManualExceptionManifest.NewManualExceptionManifest("code-741", "name-390");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualExceptionManifestEngineKeepsResultsWithinBounds();
    }
}
