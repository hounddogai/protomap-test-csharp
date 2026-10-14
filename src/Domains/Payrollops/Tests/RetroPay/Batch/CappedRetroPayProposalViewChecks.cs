namespace Acme.Workforce.Payrollops.Tests.RetroPay.Batch;

using Acme.Workforce.Payrollops.RetroPay.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedRetroPayProposalViewChecks
{
    public static void CappedRetroPayProposalViewShowsTheStatusInCapitals()
    {
        var item = CappedRetroPayProposal.NewCappedRetroPayProposal("code-560", "title-351");
        var view = CappedRetroPayProposalView.ToCappedRetroPayProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(CappedRetroPayProposalView.ToCappedRetroPayProposalViews(new List<CappedRetroPayProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        CappedRetroPayProposalViewShowsTheStatusInCapitals();
    }
}
