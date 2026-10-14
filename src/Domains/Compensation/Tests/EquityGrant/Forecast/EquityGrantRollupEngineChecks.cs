namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantRollupEngineChecks
{
    public static void EquityGrantRollupEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantRollupEngine.DefaultEquityGrantRollupEngine(Flags.NewRegistry());
        var item = EquityGrantRollup.NewEquityGrantRollup("code-624", "name-214");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantRollupEngineKeepsResultsWithinBounds();
    }
}
