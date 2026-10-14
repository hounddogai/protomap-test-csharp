namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Baseline;

using Acme.Workforce.Contractors.StatementOfWork.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkSegmentEngineChecks
{
    public static void ScheduledStatementOfWorkSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledStatementOfWorkSegmentEngine.DefaultScheduledStatementOfWorkSegmentEngine();
        var item = ScheduledStatementOfWorkSegment.NewScheduledStatementOfWorkSegment("code-861", "name-956");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkSegmentEngineKeepsResultsWithinBounds();
    }
}
