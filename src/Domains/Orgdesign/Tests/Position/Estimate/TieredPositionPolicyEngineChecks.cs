namespace Acme.Workforce.Orgdesign.Tests.Position.Estimate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Position.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredPositionPolicyEngineChecks
{
    public static void TieredPositionPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = TieredPositionPolicyEngine.DefaultTieredPositionPolicyEngine(Flags.NewRegistry());
        var item = TieredPositionPolicy.NewTieredPositionPolicy("code-994", "title-522");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredPositionPolicyEngineKeepsResultsWithinBounds();
    }
}
