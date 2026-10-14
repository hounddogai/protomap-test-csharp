namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PrimaryRateRequestEngineChecks
{
    public static void PrimaryRateRequestEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryRateRequestEngine.DefaultPrimaryRateRequestEngine();
        var item = PrimaryRateRequest.NewPrimaryRateRequest("code-878", "label-35");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryRateRequestEngineKeepsResultsWithinBounds();
    }
}
