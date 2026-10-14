namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Cycle;

using Acme.Workforce.Contractors.StatementOfWork.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProposalViewChecks
{
    public static void CappedStatementOfWorkProposalViewShowsTheStatusInCapitals()
    {
        var item = CappedStatementOfWorkProposal.NewCappedStatementOfWorkProposal("code-530", "name-991");
        var view = CappedStatementOfWorkProposalView.ToCappedStatementOfWorkProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedStatementOfWorkProposalView.ToCappedStatementOfWorkProposalViews(new List<CappedStatementOfWorkProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProposalViewShowsTheStatusInCapitals();
    }
}
