namespace Acme.Workforce.Payrollops.Tests.Payslip.Mapping;

using Acme.Workforce.Payrollops.Payslip.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class GlobalPayslipProposalChecks
{
    public static void GlobalPayslipProposalStartsInCreated()
    {
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("code-361", "title-296");
        Check.Equal(item.Status, GlobalPayslipProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPayslipProposalMovesToPublished()
    {
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("code-361", "title-296");
        try
        {
            item.MoveTo(GlobalPayslipProposalStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPayslipProposalStatus.Published, "status");
    }

    public static void GlobalPayslipProposalRejectsMovesFromRetired()
    {
        var item = GlobalPayslipProposal.NewGlobalPayslipProposal("code-361", "title-296");
        item.Status = GlobalPayslipProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(GlobalPayslipProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        GlobalPayslipProposalStartsInCreated();
        GlobalPayslipProposalMovesToPublished();
        GlobalPayslipProposalRejectsMovesFromRetired();
    }
}
