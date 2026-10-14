namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkTargetViewChecks
{
    public static void StatementOfWorkTargetViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkTarget.NewStatementOfWorkTarget("code-452", "title-660");
        var view = StatementOfWorkTargetView.ToStatementOfWorkTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkTargetView.ToStatementOfWorkTargetViews(new List<StatementOfWorkTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkTargetViewShowsTheStatusInCapitals();
    }
}
