namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionBundleEngineChecks
{
    public static void PositionBundleEngineKeepsResultsWithinBounds()
    {
        var engine = PositionBundleEngine.DefaultPositionBundleEngine(Flags.NewRegistry());
        var item = PositionBundle.NewPositionBundle("code-176", "name-896");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PositionBundleEngineKeepsResultsWithinBounds();
    }
}
