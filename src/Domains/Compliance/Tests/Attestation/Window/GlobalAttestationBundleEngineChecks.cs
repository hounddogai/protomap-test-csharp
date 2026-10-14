namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class GlobalAttestationBundleEngineChecks
{
    public static void GlobalAttestationBundleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalAttestationBundleEngine.DefaultGlobalAttestationBundleEngine();
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("code-368", "name-946");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalAttestationBundleEngineKeepsResultsWithinBounds();
    }
}
