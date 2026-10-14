namespace Acme.Workforce.Talent.Tests.Pool.Adjustment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class ManualPoolTierEngineChecks
{
    public static void ManualPoolTierEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPoolTierEngine.DefaultManualPoolTierEngine(Flags.NewRegistry());
        var item = ManualPoolTier.NewManualPoolTier("code-855", "name-647");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPoolTierEngineKeepsResultsWithinBounds();
    }
}
