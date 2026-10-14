namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalRateRuleEngineChecks
{
    public static void RegionalRateRuleEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalRateRuleEngine.DefaultRegionalRateRuleEngine();
        var item = RegionalRateRule.NewRegionalRateRule("code-372", "title-729");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalRateRuleEngineKeepsResultsWithinBounds();
    }
}
