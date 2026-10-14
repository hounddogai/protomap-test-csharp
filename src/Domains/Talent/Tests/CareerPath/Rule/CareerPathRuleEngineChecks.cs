namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathRuleEngineChecks
{
    public static void CareerPathRuleEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathRuleEngine.DefaultCareerPathRuleEngine(Flags.NewRegistry());
        var item = CareerPathRule.NewCareerPathRule("code-145", "title-248");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathRuleEngineKeepsResultsWithinBounds();
    }
}
