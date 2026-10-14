namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkTargetEngineChecks
{
    public static void StatementOfWorkTargetEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkTargetEngine.DefaultStatementOfWorkTargetEngine();
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("code-452", "title-660");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkTargetEngineKeepsResultsWithinBounds();
    }
}
