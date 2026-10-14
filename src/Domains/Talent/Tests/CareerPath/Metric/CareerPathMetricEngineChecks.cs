namespace Acme.Workforce.Talent.Tests.CareerPath.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class CareerPathMetricEngineChecks
{
    public static void CareerPathMetricEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathMetricEngine.DefaultCareerPathMetricEngine(Flags.NewRegistry());
        var item = CareerPathMetric.NewCareerPathMetric("code-12", "label-973");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathMetricEngineKeepsResultsWithinBounds();
    }
}
