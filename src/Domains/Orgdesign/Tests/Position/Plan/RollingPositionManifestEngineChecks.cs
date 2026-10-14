namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RollingPositionManifestEngineChecks
{
    public static void RollingPositionManifestEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPositionManifestEngine.DefaultRollingPositionManifestEngine(Flags.NewRegistry());
        var item = RollingPositionManifest.NewRollingPositionManifest("code-469", "name-133");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPositionManifestEngineKeepsResultsWithinBounds();
    }
}
