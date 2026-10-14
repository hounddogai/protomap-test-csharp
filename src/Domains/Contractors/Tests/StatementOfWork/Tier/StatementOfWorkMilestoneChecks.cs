namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkMilestoneChecks
{
    public static void StatementOfWorkMilestoneStartsInNew()
    {
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("code-153", "name-258");
        Check.Equal(item.Status, StatementOfWorkMilestoneStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkMilestoneMovesToRunning()
    {
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("code-153", "name-258");
        try
        {
            item.MoveTo(StatementOfWorkMilestoneStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkMilestoneStatus.Running, "status");
    }

    public static void StatementOfWorkMilestoneRejectsMovesFromFailed()
    {
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("code-153", "name-258");
        item.Status = StatementOfWorkMilestoneStatus.Failed;
        Check.Fails(() => item.MoveTo(StatementOfWorkMilestoneStatus.New), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkMilestoneStartsInNew();
        StatementOfWorkMilestoneMovesToRunning();
        StatementOfWorkMilestoneRejectsMovesFromFailed();
    }
}
