namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPayRunTargetEngineChecks
{
    public static void LegacyPayRunTargetEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyPayRunTargetEngine.DefaultLegacyPayRunTargetEngine(Flags.NewRegistry());
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("code-587", "name-357");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyPayRunTargetEngineKeepsResultsWithinBounds();
    }
}
