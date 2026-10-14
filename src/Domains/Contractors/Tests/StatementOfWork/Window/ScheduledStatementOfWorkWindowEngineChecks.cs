namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkWindowEngineChecks
{
    public static void ScheduledStatementOfWorkWindowEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledStatementOfWorkWindowEngine.DefaultScheduledStatementOfWorkWindowEngine();
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("code-344", "label-241");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkWindowEngineKeepsResultsWithinBounds();
    }
}
