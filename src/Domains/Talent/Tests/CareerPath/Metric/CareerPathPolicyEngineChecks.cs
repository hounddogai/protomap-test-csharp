namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathPolicyEngineChecks
{
    public static void CareerPathPolicyEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathPolicyEngine.DefaultCareerPathPolicyEngine(Flags.NewRegistry());
        var item = CareerPathPolicy.NewCareerPathPolicy("code-309", "label-28");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathPolicyEngineKeepsResultsWithinBounds();
    }
}
