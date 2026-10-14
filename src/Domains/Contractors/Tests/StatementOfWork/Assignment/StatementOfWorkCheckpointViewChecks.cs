namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkCheckpointViewChecks
{
    public static void StatementOfWorkCheckpointViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkCheckpoint.NewStatementOfWorkCheckpoint("code-9", "title-42");
        var view = StatementOfWorkCheckpointView.ToStatementOfWorkCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkCheckpointView.ToStatementOfWorkCheckpointViews(new List<StatementOfWorkCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkCheckpointViewShowsTheStatusInCapitals();
    }
}
