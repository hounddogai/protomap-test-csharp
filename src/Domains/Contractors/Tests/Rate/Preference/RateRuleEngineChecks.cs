namespace Acme.Workforce.Contractors.Tests.Rate.Preference;

using Acme.Workforce.Contractors.Rate.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class RateRuleEngineChecks
{
    public static void RateRuleEngineKeepsResultsWithinBounds()
    {
        var engine = RateRuleEngine.DefaultRateRuleEngine(Flags.NewRegistry());
        var item = RateRule.NewRateRule("code-87", "name-441");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateRuleEngineKeepsResultsWithinBounds();
    }
}
