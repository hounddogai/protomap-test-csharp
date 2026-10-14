namespace Acme.Workforce.Talent.Tests.CareerPath.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class WeightedCareerPathOverrideEngineChecks
{
    public static void WeightedCareerPathOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedCareerPathOverrideEngine.DefaultWeightedCareerPathOverrideEngine(Flags.NewRegistry());
        var item = WeightedCareerPathOverride.NewWeightedCareerPathOverride("code-915", "title-538");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedCareerPathOverrideEngineKeepsResultsWithinBounds();
    }
}
