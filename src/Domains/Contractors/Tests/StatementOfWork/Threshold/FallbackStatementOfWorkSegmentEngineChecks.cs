namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FallbackStatementOfWorkSegmentEngineChecks
{
    public static void FallbackStatementOfWorkSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackStatementOfWorkSegmentEngine.DefaultFallbackStatementOfWorkSegmentEngine();
        var item = FallbackStatementOfWorkSegment.NewFallbackStatementOfWorkSegment("code-187", "name-726");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackStatementOfWorkSegmentEngineKeepsResultsWithinBounds();
    }
}
