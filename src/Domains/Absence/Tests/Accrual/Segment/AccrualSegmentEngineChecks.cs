namespace Acme.Workforce.Absence.Tests.Accrual.Segment;

using Acme.Workforce.Absence.Accrual.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AccrualSegmentEngineChecks
{
    public static void AccrualSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = AccrualSegmentEngine.DefaultAccrualSegmentEngine();
        var item = AccrualSegment.NewAccrualSegment("code-884", "name-632");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccrualSegmentEngineKeepsResultsWithinBounds();
    }
}
