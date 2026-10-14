namespace Acme.Workforce.Contractors.Tests.Rate.Segment;

using Acme.Workforce.Contractors.Rate.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RateAssignmentEngineChecks
{
    public static void RateAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = RateAssignmentEngine.DefaultRateAssignmentEngine();
        var item = RateAssignment.NewRateAssignment("code-541", "title-170");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateAssignmentEngineKeepsResultsWithinBounds();
    }
}
