namespace Acme.Workforce.Talent.Tests.CareerPath.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class CareerPathVarianceEngineChecks
{
    public static void CareerPathVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathVarianceEngine.DefaultCareerPathVarianceEngine(Flags.NewRegistry());
        var item = CareerPathVariance.NewCareerPathVariance("code-416", "title-157");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathVarianceEngineKeepsResultsWithinBounds();
    }
}
