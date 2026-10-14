namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Metric;

using Acme.Workforce.Contractors.StatementOfWork.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkMetricEngineChecks
{
    public static void DefaultStatementOfWorkMetricEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultStatementOfWorkMetricEngine.DefaultDefaultStatementOfWorkMetricEngine(Flags.NewRegistry());
        var item = DefaultStatementOfWorkMetric.NewDefaultStatementOfWorkMetric("code-732", "name-210");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkMetricEngineKeepsResultsWithinBounds();
    }
}
