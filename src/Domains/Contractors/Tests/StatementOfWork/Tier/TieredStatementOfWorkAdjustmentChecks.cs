namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAdjustmentChecks
{
    public static void TieredStatementOfWorkAdjustmentStartsInProposed()
    {
        var item = TieredStatementOfWorkAdjustment.NewTieredStatementOfWorkAdjustment("code-219", "label-311");
        Check.Equal(item.Status, TieredStatementOfWorkAdjustmentStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredStatementOfWorkAdjustmentMovesToApproved()
    {
        var item = TieredStatementOfWorkAdjustment.NewTieredStatementOfWorkAdjustment("code-219", "label-311");
        try
        {
            item.MoveTo(TieredStatementOfWorkAdjustmentStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredStatementOfWorkAdjustmentStatus.Approved, "status");
    }

    public static void TieredStatementOfWorkAdjustmentRejectsMovesFromWithdrawn()
    {
        var item = TieredStatementOfWorkAdjustment.NewTieredStatementOfWorkAdjustment("code-219", "label-311");
        item.Status = TieredStatementOfWorkAdjustmentStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TieredStatementOfWorkAdjustmentStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkAdjustmentStartsInProposed();
        TieredStatementOfWorkAdjustmentMovesToApproved();
        TieredStatementOfWorkAdjustmentRejectsMovesFromWithdrawn();
    }
}
