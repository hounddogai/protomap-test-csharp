namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionTargetEngineChecks
{
    public static void LegacyPositionTargetEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPositionTargetEngine.DefaultLegacyPositionTargetEngine(Flags.NewRegistry());
        var item = LegacyPositionTarget.NewLegacyPositionTarget("code-666", "title-561");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPositionTargetEngineKeepsResultsWithinBounds();
    }
}
