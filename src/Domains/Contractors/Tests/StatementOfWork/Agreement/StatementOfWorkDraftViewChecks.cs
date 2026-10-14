namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class StatementOfWorkDraftViewChecks
{
    public static void StatementOfWorkDraftViewShowsTheStatusInCapitals()
    {
        var item = StatementOfWorkDraft.NewStatementOfWorkDraft("code-536", "label-90");
        var view = StatementOfWorkDraftView.ToStatementOfWorkDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StatementOfWorkDraftView.ToStatementOfWorkDraftViews(new List<StatementOfWorkDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StatementOfWorkDraftViewShowsTheStatusInCapitals();
    }
}
