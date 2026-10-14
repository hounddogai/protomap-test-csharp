namespace Acme.Workforce.Orgdesign.Tests.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

/// <summary>Checks of rolling position estimate code, run by the project's runner.</summary>
public static class LegacyPositionEstimateEngineChecks
{
    public static void LegacyPositionEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPositionEstimateEngine.DefaultLegacyPositionEstimateEngine(Flags.NewRegistry());
        var item = LegacyPositionEstimate.NewLegacyPositionEstimate("code-588", "label-249");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPositionEstimateEngineKeepsResultsWithinBounds();
    }
}
