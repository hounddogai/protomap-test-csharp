namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkMetricEngineChecks
{
    public static void StatementOfWorkMetricEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkMetricEngine.DefaultStatementOfWorkMetricEngine();
        var item = StatementOfWorkMetric.NewStatementOfWorkMetric("code-737", "title-546");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkMetricEngineKeepsResultsWithinBounds();
    }
}
