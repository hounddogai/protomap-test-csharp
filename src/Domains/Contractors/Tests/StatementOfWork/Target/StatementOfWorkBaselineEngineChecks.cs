namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StatementOfWorkBaselineEngineChecks
{
    public static void StatementOfWorkBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkBaselineEngine.DefaultStatementOfWorkBaselineEngine();
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("code-45", "label-197");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkBaselineEngineKeepsResultsWithinBounds();
    }
}
