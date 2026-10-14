namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkAuditViewChecks
{
    public static void StatementOfWorkAuditViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkAudit.NewStatementOfWorkAudit("code-506", "name-442");
        var view = StatementOfWorkAuditView.ToStatementOfWorkAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkAuditView.ToStatementOfWorkAuditViews(new List<StatementOfWorkAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkAuditViewShowsTheStatusInCapitals();
    }
}
