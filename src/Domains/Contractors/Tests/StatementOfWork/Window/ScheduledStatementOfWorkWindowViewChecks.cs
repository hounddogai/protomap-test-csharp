namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkWindowViewChecks
{
    public static void ScheduledStatementOfWorkWindowViewShowsTheStatusInCapitals()
    {
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("code-344", "label-241");
        var view = ScheduledStatementOfWorkWindowView.ToScheduledStatementOfWorkWindowView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledStatementOfWorkWindowView.ToScheduledStatementOfWorkWindowViews(new List<ScheduledStatementOfWorkWindow> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkWindowViewShowsTheStatusInCapitals();
    }
}
