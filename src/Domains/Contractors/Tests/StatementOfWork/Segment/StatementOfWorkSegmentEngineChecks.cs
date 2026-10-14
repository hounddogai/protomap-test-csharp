namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Segment;

using Acme.Workforce.Contractors.StatementOfWork.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class StatementOfWorkSegmentEngineChecks
{
    public static void StatementOfWorkSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkSegmentEngine.DefaultStatementOfWorkSegmentEngine();
        var item = StatementOfWorkSegment.NewStatementOfWorkSegment("code-116", "name-24");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkSegmentEngineKeepsResultsWithinBounds();
    }
}
