namespace Acme.Workforce.Talent.Tests.Mobility.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Mobility.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GlobalMobilityRuleEngineChecks
{
    public static void GlobalMobilityRuleEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalMobilityRuleEngine.DefaultGlobalMobilityRuleEngine(Flags.NewRegistry());
        var item = GlobalMobilityRule.NewGlobalMobilityRule("code-900", "title-815");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalMobilityRuleEngineKeepsResultsWithinBounds();
    }
}
