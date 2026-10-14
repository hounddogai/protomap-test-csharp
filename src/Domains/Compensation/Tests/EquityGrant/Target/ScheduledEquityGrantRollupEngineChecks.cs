namespace Acme.Workforce.Compensation.Tests.EquityGrant.Target;

using Acme.Workforce.Compensation.EquityGrant.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledEquityGrantRollupEngineChecks
{
    public static void ScheduledEquityGrantRollupEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledEquityGrantRollupEngine.DefaultScheduledEquityGrantRollupEngine(Flags.NewRegistry());
        var item = ScheduledEquityGrantRollup.NewScheduledEquityGrantRollup("code-697", "label-663");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledEquityGrantRollupEngineKeepsResultsWithinBounds();
    }
}
