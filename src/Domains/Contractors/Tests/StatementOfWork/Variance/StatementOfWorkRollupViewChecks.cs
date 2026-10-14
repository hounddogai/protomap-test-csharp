namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkRollupViewChecks
{
    public static void StatementOfWorkRollupViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkRollup.NewStatementOfWorkRollup("code-380", "label-820");
        var view = StatementOfWorkRollupView.ToStatementOfWorkRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkRollupView.ToStatementOfWorkRollupViews(new List<StatementOfWorkRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkRollupViewShowsTheStatusInCapitals();
    }
}
