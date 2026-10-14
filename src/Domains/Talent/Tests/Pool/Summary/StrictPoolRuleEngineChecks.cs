namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StrictPoolRuleEngineChecks
{
    public static void StrictPoolRuleEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPoolRuleEngine.DefaultStrictPoolRuleEngine();
        var item = StrictPoolRule.NewStrictPoolRule("code-155", "title-975");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPoolRuleEngineKeepsResultsWithinBounds();
    }
}
