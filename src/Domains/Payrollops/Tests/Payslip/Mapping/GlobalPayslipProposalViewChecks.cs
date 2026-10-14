namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalPayslipProposalViewChecks
{
    public static void GlobalPayslipProposalViewShowsTheStatusInCapitals()
    {
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("code-361", "title-296");
        var view = GlobalPayslipProposalView.ToGlobalPayslipProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalPayslipProposalView.ToGlobalPayslipProposalViews(new List<GlobalPayslipProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalPayslipProposalViewShowsTheStatusInCapitals();
    }
}
