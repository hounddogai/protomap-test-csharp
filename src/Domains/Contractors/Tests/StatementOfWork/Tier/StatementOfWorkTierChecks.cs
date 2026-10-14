namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkTierChecks
{
    public static void StatementOfWorkTierStartsInDraft()
    {
        var item = StatementOfWorkTier.NewStatementOfWorkTier("code-871", "label-379");
        Check.Equal(item.Status, StatementOfWorkTierStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkTierMovesToActive()
    {
        var item = StatementOfWorkTier.NewStatementOfWorkTier("code-871", "label-379");
        try
        {
            item.MoveTo(StatementOfWorkTierStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkTierStatus.Active, "status");
    }

    public static void StatementOfWorkTierRejectsMovesFromClosed()
    {
        var item = StatementOfWorkTier.NewStatementOfWorkTier("code-871", "label-379");
        item.Status = StatementOfWorkTierStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkTierStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkTierStartsInDraft();
        StatementOfWorkTierMovesToActive();
        StatementOfWorkTierRejectsMovesFromClosed();
    }
}
