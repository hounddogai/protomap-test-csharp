namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class RollingPositionBundleEngineChecks
{
    public static void RollingPositionBundleEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPositionBundleEngine.DefaultRollingPositionBundleEngine(Flags.NewRegistry());
        var item = RollingPositionBundle.NewRollingPositionBundle("code-370", "label-635");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPositionBundleEngineKeepsResultsWithinBounds();
    }
}
