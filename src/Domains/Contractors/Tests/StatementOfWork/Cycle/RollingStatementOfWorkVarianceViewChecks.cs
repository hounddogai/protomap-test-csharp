namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class RollingStatementOfWorkVarianceViewChecks
{
    public static void RollingStatementOfWorkVarianceViewShowsTheStatusInCapitals()
    {
        var item = RollingStatementOfWorkVariance.NewRollingStatementOfWorkVariance("code-565", "title-301");
        var view = RollingStatementOfWorkVarianceView.ToRollingStatementOfWorkVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingStatementOfWorkVarianceView.ToRollingStatementOfWorkVarianceViews(new List<RollingStatementOfWorkVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingStatementOfWorkVarianceViewShowsTheStatusInCapitals();
    }
}
