namespace Acme.Workforce.Talent.Tests.CareerPath.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.CareerPath.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class CareerPathSegmentEngineChecks
{
    public static void CareerPathSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = CareerPathSegmentEngine.DefaultCareerPathSegmentEngine(Flags.NewRegistry());
        var item = CareerPathSegment.NewCareerPathSegment("code-991", "name-967");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CareerPathSegmentEngineKeepsResultsWithinBounds();
    }
}
