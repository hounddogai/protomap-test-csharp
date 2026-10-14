namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkRequestEngineChecks
{
    public static void StatementOfWorkRequestEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkRequestEngine.DefaultStatementOfWorkRequestEngine(Flags.NewRegistry());
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("code-193", "title-35");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkRequestEngineKeepsResultsWithinBounds();
    }
}
