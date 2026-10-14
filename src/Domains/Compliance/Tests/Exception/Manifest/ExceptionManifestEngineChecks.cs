namespace Acme.Workforce.Compliance.Tests.Exception.Manifest;

using Acme.Workforce.Compliance.Exception.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ExceptionManifestEngineChecks
{
    public static void ExceptionManifestEngineKeepsResultsWithinBounds()
    {
        var engine = ExceptionManifestEngine.DefaultExceptionManifestEngine(Flags.NewRegistry());
        var item = ExceptionManifest.NewExceptionManifest("code-333", "title-102");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ExceptionManifestEngineKeepsResultsWithinBounds();
    }
}
