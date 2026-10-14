namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Allocation;

using Acme.Workforce.Contractors.StatementOfWork.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class StatementOfWorkAdjustmentChecks
{
    public static void StatementOfWorkAdjustmentStartsInPlanned()
    {
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("code-134", "title-646");
        Check.Equal(item.Status, StatementOfWorkAdjustmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkAdjustmentMovesToScheduled()
    {
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("code-134", "title-646");
        try
        {
            item.MoveTo(StatementOfWorkAdjustmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkAdjustmentStatus.Scheduled, "status");
    }

    public static void StatementOfWorkAdjustmentRejectsMovesFromCancelled()
    {
        var item = StatementOfWorkAdjustment.NewStatementOfWorkAdjustment("code-134", "title-646");
        item.Status = StatementOfWorkAdjustmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StatementOfWorkAdjustmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkAdjustmentStartsInPlanned();
        StatementOfWorkAdjustmentMovesToScheduled();
        StatementOfWorkAdjustmentRejectsMovesFromCancelled();
    }
}
