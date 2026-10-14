namespace Acme.Workforce.Payrollops.Tests.Payslip.Assignment;

using Acme.Workforce.Payrollops.Payslip.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BatchedPayslipProposalChecks
{
    public static void BatchedPayslipProposalStartsInCreated()
    {
        var item = BatchedPayslipProposal.NewBatchedPayslipProposal("code-66", "name-600");
        Check.Equal(item.Status, BatchedPayslipProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPayslipProposalMovesToRetired()
    {
        var item = BatchedPayslipProposal.NewBatchedPayslipProposal("code-66", "name-600");
        try
        {
            item.MoveTo(BatchedPayslipProposalStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPayslipProposalStatus.Retired, "status");
    }

    public static void BatchedPayslipProposalRejectsMovesFromRetired()
    {
        var item = BatchedPayslipProposal.NewBatchedPayslipProposal("code-66", "name-600");
        item.Status = BatchedPayslipProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedPayslipProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedPayslipProposalStartsInCreated();
        BatchedPayslipProposalMovesToRetired();
        BatchedPayslipProposalRejectsMovesFromRetired();
    }
}
