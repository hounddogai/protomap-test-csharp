namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkTargetChecks
{
    public static void LegacyStatementOfWorkTargetStartsInProposed()
    {
        var item = LegacyStatementOfWorkTarget.NewLegacyStatementOfWorkTarget("code-447", "name-749");
        Check.Equal(item.Status, LegacyStatementOfWorkTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyStatementOfWorkTargetMovesToApproved()
    {
        var item = LegacyStatementOfWorkTarget.NewLegacyStatementOfWorkTarget("code-447", "name-749");
        try
        {
            item.MoveTo(LegacyStatementOfWorkTargetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyStatementOfWorkTargetStatus.Approved, "status");
    }

    public static void LegacyStatementOfWorkTargetRejectsMovesFromWithdrawn()
    {
        var item = LegacyStatementOfWorkTarget.NewLegacyStatementOfWorkTarget("code-447", "name-749");
        item.Status = LegacyStatementOfWorkTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LegacyStatementOfWorkTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkTargetStartsInProposed();
        LegacyStatementOfWorkTargetMovesToApproved();
        LegacyStatementOfWorkTargetRejectsMovesFromWithdrawn();
    }
}
