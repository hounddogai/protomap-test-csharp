namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkRequestViewChecks
{
    public static void StatementOfWorkRequestViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkRequest.NewStatementOfWorkRequest("code-193", "title-35");
        var view = StatementOfWorkRequestView.ToStatementOfWorkRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkRequestView.ToStatementOfWorkRequestViews(new List<StatementOfWorkRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkRequestViewShowsTheStatusInCapitals();
    }
}
