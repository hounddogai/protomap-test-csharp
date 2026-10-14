namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkQuotaViewChecks
{
    public static void StatementOfWorkQuotaViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("code-276", "label-356");
        var view = StatementOfWorkQuotaView.ToStatementOfWorkQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkQuotaView.ToStatementOfWorkQuotaViews(new List<StatementOfWorkQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkQuotaViewShowsTheStatusInCapitals();
    }
}
