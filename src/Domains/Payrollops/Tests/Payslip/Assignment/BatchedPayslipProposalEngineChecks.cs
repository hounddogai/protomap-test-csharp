namespace Acme.Workforce.Payrollops.Tests.Payslip.Assignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Payslip.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BatchedPayslipProposalEngineChecks
{
    public static void BatchedPayslipProposalEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPayslipProposalEngine.DefaultBatchedPayslipProposalEngine(Flags.NewRegistry());
        var item = BatchedPayslipProposal.NewBatchedPayslipProposal("code-66", "name-600");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPayslipProposalEngineKeepsResultsWithinBounds();
    }
}
