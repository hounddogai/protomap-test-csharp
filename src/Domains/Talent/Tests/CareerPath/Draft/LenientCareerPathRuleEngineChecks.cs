namespace Acme.Workforce.Talent.Tests.CareerPath.Draft;

using Acme.Workforce.Talent.CareerPath.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientCareerPathRuleEngineChecks
{
    public static void LenientCareerPathRuleEngineKeepsResultsWithinBounds()
    {
        var engine = LenientCareerPathRuleEngine.DefaultLenientCareerPathRuleEngine();
        var item = LenientCareerPathRule.NewLenientCareerPathRule("code-837", "name-685");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientCareerPathRuleEngineKeepsResultsWithinBounds();
    }
}
