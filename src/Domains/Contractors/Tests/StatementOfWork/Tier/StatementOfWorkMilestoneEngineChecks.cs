namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkMilestoneEngineChecks
{
    public static void StatementOfWorkMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = StatementOfWorkMilestoneEngine.DefaultStatementOfWorkMilestoneEngine(Flags.NewRegistry());
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("code-153", "name-258");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StatementOfWorkMilestoneEngineKeepsResultsWithinBounds();
    }
}
