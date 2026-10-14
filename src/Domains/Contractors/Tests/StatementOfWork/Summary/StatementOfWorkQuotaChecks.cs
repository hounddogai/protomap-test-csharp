namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkQuotaChecks
{
    public static void StatementOfWorkQuotaStartsInDraft()
    {
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("code-276", "label-356");
        Check.Equal(item.Status, StatementOfWorkQuotaStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkQuotaMovesToActive()
    {
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("code-276", "label-356");
        try
        {
            item.MoveTo(StatementOfWorkQuotaStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkQuotaStatus.Active, "status");
    }

    public static void StatementOfWorkQuotaRejectsMovesFromClosed()
    {
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("code-276", "label-356");
        item.Status = StatementOfWorkQuotaStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkQuotaStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkQuotaStartsInDraft();
        StatementOfWorkQuotaMovesToActive();
        StatementOfWorkQuotaRejectsMovesFromClosed();
    }
}
