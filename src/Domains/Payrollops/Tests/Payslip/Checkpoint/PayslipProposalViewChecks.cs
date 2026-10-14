namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class PayslipProposalViewChecks
{
    public static void PayslipProposalViewShowsTheStatusInCapitals()
    {
        var item = PayslipProposal.NewPayslipProposal("code-18", "title-345");
        var view = PayslipProposalView.ToPayslipProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayslipProposalView.ToPayslipProposalViews(new List<PayslipProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayslipProposalViewShowsTheStatusInCapitals();
    }
}
