namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Bundle;

using Acme.Workforce.Contractors.StatementOfWork.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class StatementOfWorkBundleViewChecks
{
    public static void StatementOfWorkBundleViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkBundle.NewStatementOfWorkBundle("code-551", "name-155");
        var view = StatementOfWorkBundleView.ToStatementOfWorkBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkBundleView.ToStatementOfWorkBundleViews(new List<StatementOfWorkBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkBundleViewShowsTheStatusInCapitals();
    }
}
