namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class DefaultStatementOfWorkAuditViewChecks
{
    public static void DefaultStatementOfWorkAuditViewShowsTheStatusInCapitals()
    {
        var item = DefaultStatementOfWorkAudit.NewDefaultStatementOfWorkAudit("code-643", "name-387");
        var view = DefaultStatementOfWorkAuditView.ToDefaultStatementOfWorkAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultStatementOfWorkAuditView.ToDefaultStatementOfWorkAuditViews(new List<DefaultStatementOfWorkAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultStatementOfWorkAuditViewShowsTheStatusInCapitals();
    }
}
