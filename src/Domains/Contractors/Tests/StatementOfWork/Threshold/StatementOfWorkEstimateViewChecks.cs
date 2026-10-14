namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkEstimateViewChecks
{
    public static void StatementOfWorkEstimateViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkEstimate.NewStatementOfWorkEstimate("code-18", "label-543");
        var view = StatementOfWorkEstimateView.ToStatementOfWorkEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkEstimateView.ToStatementOfWorkEstimateViews(new List<StatementOfWorkEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkEstimateViewShowsTheStatusInCapitals();
    }
}
