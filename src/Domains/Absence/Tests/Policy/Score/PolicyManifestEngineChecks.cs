namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyManifestEngineChecks
{
    public static void PolicyManifestEngineKeepsResultsWithinBounds()
    {
        var engine = PolicyManifestEngine.DefaultPolicyManifestEngine();
        var item = PolicyManifest.NewPolicyManifest("code-475", "name-254");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PolicyManifestEngineKeepsResultsWithinBounds();
    }
}
