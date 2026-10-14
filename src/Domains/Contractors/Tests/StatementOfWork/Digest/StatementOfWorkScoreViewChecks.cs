namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkScoreViewChecks
{
    public static void StatementOfWorkScoreViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkScore.NewStatementOfWorkScore("code-628", "name-475");
        var view = StatementOfWorkScoreView.ToStatementOfWorkScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkScoreView.ToStatementOfWorkScoreViews(new List<StatementOfWorkScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkScoreViewShowsTheStatusInCapitals();
    }
}
