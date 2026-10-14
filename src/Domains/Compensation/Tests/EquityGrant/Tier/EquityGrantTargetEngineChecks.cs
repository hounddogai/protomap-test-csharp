namespace Acme.Workforce.Compensation.Tests.EquityGrant.Tier;

using Acme.Workforce.Compensation.EquityGrant.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class EquityGrantTargetEngineChecks
{
    public static void EquityGrantTargetEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantTargetEngine.DefaultEquityGrantTargetEngine(Flags.NewRegistry());
        var item = EquityGrantTarget.NewEquityGrantTarget("code-930", "label-40");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantTargetEngineKeepsResultsWithinBounds();
    }
}
