namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Rule;

using Acme.Workforce.Contractors.StatementOfWork.Rule;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkMetricEngineChecks
{
    public static void StrictStatementOfWorkMetricEngineKeepsResultsWithinBounds()
    {
        var engine = StrictStatementOfWorkMetricEngine.DefaultStrictStatementOfWorkMetricEngine(Flags.NewRegistry());
        var item = StrictStatementOfWorkMetric.NewStrictStatementOfWorkMetric("code-137", "name-494");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictStatementOfWorkMetricEngineKeepsResultsWithinBounds();
    }
}
