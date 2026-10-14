namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedStatementOfWorkBundleViewChecks
{
    public static void WeightedStatementOfWorkBundleViewShowsTheStatusInCapitals()
    {
        var item = WeightedStatementOfWorkBundle.NewWeightedStatementOfWorkBundle("code-309", "name-146");
        var view = WeightedStatementOfWorkBundleView.ToWeightedStatementOfWorkBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedStatementOfWorkBundleView.ToWeightedStatementOfWorkBundleViews(new List<WeightedStatementOfWorkBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedStatementOfWorkBundleViewShowsTheStatusInCapitals();
    }
}
