namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedStatementOfWorkExceptionViewChecks
{
    public static void CappedStatementOfWorkExceptionViewShowsTheStatusInCapitals()
    {
        var item = CappedStatementOfWorkException.NewCappedStatementOfWorkException("code-789", "name-124");
        var view = CappedStatementOfWorkExceptionView.ToCappedStatementOfWorkExceptionView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedStatementOfWorkExceptionView.ToCappedStatementOfWorkExceptionViews(new List<CappedStatementOfWorkException> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkExceptionViewShowsTheStatusInCapitals();
    }
}
