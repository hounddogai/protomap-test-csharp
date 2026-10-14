namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkExceptionViewChecks
{
    public static void StatementOfWorkExceptionViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkException.NewStatementOfWorkException("code-34", "name-98");
        var view = StatementOfWorkExceptionView.ToStatementOfWorkExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkExceptionView.ToStatementOfWorkExceptionViews(new List<StatementOfWorkException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkExceptionViewShowsTheStatusInCapitals();
    }
}
