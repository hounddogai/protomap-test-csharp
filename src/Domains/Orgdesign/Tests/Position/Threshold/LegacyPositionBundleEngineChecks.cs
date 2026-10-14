namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyPositionBundleEngineChecks
{
    public static void LegacyPositionBundleEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPositionBundleEngine.DefaultLegacyPositionBundleEngine(Flags.NewRegistry());
        var item = LegacyPositionBundle.NewLegacyPositionBundle("code-673", "label-832");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPositionBundleEngineKeepsResultsWithinBounds();
    }
}
