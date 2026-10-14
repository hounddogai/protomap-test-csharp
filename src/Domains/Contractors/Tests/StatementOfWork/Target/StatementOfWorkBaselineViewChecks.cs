namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Target;

using Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class StatementOfWorkBaselineViewChecks
{
    public static void StatementOfWorkBaselineViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkBaseline.NewStatementOfWorkBaseline("code-45", "label-197");
        var view = StatementOfWorkBaselineView.ToStatementOfWorkBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkBaselineView.ToStatementOfWorkBaselineViews(new List<StatementOfWorkBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkBaselineViewShowsTheStatusInCapitals();
    }
}
