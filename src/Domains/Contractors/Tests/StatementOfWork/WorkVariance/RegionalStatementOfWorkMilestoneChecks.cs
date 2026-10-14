namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkMilestoneChecks
{
    public static void RegionalStatementOfWorkMilestoneStartsInProposed()
    {
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("code-395", "title-864");
        Check.Equal(item.Status, RegionalStatementOfWorkMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalStatementOfWorkMilestoneMovesToRejected()
    {
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("code-395", "title-864");
        try
        {
            item.MoveTo(RegionalStatementOfWorkMilestoneStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalStatementOfWorkMilestoneStatus.Rejected, "status");
    }

    public static void RegionalStatementOfWorkMilestoneRejectsMovesFromWithdrawn()
    {
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("code-395", "title-864");
        item.Status = RegionalStatementOfWorkMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RegionalStatementOfWorkMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkMilestoneStartsInProposed();
        RegionalStatementOfWorkMilestoneMovesToRejected();
        RegionalStatementOfWorkMilestoneRejectsMovesFromWithdrawn();
    }
}
