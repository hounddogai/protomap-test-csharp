namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Variance;

using Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StatementOfWorkVarianceViewChecks
{
    public static void StatementOfWorkVarianceViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkVariance.NewStatementOfWorkVariance("code-1", "title-662");
        var view = StatementOfWorkVarianceView.ToStatementOfWorkVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkVarianceView.ToStatementOfWorkVarianceViews(new List<StatementOfWorkVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkVarianceViewShowsTheStatusInCapitals();
    }
}
