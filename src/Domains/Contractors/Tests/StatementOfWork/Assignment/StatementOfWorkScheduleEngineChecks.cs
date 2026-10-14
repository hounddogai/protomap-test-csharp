namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkScheduleEngineChecks
{
    public static void StatementOfWorkScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkScheduleEngine.DefaultStatementOfWorkScheduleEngine(Flags.NewRegistry());
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("code-569", "name-787");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkScheduleEngineKeepsResultsWithinBounds();
    }
}
